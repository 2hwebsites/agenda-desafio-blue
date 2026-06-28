using System.Text.Json;
using Agenda.Contracts;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agenda.Worker;

public sealed class ContactCreatedConsumer(
    IOptions<RabbitMqOptions> options,
    ILogger<ContactCreatedConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions _opts = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        var delay = TimeSpan.FromSeconds(2);
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await SetupConsumerAsync(ct);
                logger.LogInformation(
                    "Connected to RabbitMQ at {Host}:{Port}. Consuming from queue '{Queue}'",
                    _opts.Host, _opts.Port, MessagingConstants.Queue);
                return;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning(ex,
                    "RabbitMQ unavailable (attempt {Attempt}/{Max}). Retrying in {Delay}s...",
                    attempt, maxAttempts, delay.TotalSeconds);
                try
                {
                    await Task.Delay(delay, ct);
                }
                catch (OperationCanceledException) { return; }

                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }

        logger.LogError(
            "Could not connect to RabbitMQ after {Max} attempts. Consumer will not start.", maxAttempts);
    }

    private async Task SetupConsumerAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _opts.Host,
            Port = _opts.Port,
            UserName = _opts.Username,
            Password = _opts.Password,
            AutomaticRecoveryEnabled = true,
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.ExchangeDeclareAsync(
            MessagingConstants.Exchange, ExchangeType.Topic,
            durable: true, autoDelete: false, cancellationToken: ct);

        await _channel.QueueDeclareAsync(
            MessagingConstants.Queue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: ct);

        await _channel.QueueBindAsync(
            MessagingConstants.Queue, MessagingConstants.Exchange,
            MessagingConstants.RoutingKey, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            MessagingConstants.Queue, autoAck: false, consumer, ct);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var @event = JsonSerializer.Deserialize<ContactCreatedIntegrationEvent>(
                body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

            logger.LogInformation(
                "Sending welcome email to {Email} for contact {Name} (Id: {Id})",
                @event.Email, @event.Name, @event.Id);
            // TODO: call real email service here

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ContactCreatedIntegrationEvent — discarding message");
            // requeue: false — discard to avoid poison-pill loop
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}
