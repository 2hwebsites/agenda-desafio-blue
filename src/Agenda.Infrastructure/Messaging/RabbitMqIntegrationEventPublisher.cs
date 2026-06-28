using System.Text.Json;
using Agenda.Application.Abstractions.Messaging;
using Agenda.Contracts;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Agenda.Infrastructure.Messaging;

public sealed class RabbitMqIntegrationEventPublisher(
    RabbitMqConnection connection,
    ILogger<RabbitMqIntegrationEventPublisher> logger) : IIntegrationEventPublisher, IAsyncDisposable
{
    private IChannel? _channel;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(@event);
            var props = new BasicProperties { Persistent = true };

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _channel ??= await OpenChannelAsync(cancellationToken);
                await _channel.BasicPublishAsync(
                    MessagingConstants.Exchange, routingKey,
                    mandatory: false, basicProperties: props, body: body,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }

            logger.LogDebug(
                "Published {EventType} to exchange '{Exchange}' with routing key '{RoutingKey}'",
                typeof(T).Name, MessagingConstants.Exchange, routingKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish {EventType} to RabbitMQ — event dropped",
                typeof(T).Name);
        }
    }

    private async Task<IChannel> OpenChannelAsync(CancellationToken ct)
    {
        var channel = await connection.CreateChannelAsync(ct);
        await channel.ExchangeDeclareAsync(
            MessagingConstants.Exchange, ExchangeType.Topic,
            durable: true, autoDelete: false, cancellationToken: ct);
        return channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}
