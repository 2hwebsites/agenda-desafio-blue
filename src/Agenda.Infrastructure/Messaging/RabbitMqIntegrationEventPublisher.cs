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
            var channel = await GetChannelAsync(cancellationToken);
            var body = JsonSerializer.SerializeToUtf8Bytes(@event);
            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                MessagingConstants.Exchange, routingKey,
                mandatory: false, basicProperties: props, body: body,
                cancellationToken: cancellationToken);

            logger.LogDebug(
                "Published {EventType} to exchange '{Exchange}' with routing key '{RoutingKey}'",
                typeof(T).Name, MessagingConstants.Exchange, routingKey);
        }
        catch (Exception ex)
        {
            // best-effort: log and don't propagate so the HTTP request is not affected
            logger.LogError(ex,
                "Failed to publish {EventType} to RabbitMQ — event dropped",
                typeof(T).Name);
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken ct)
    {
        if (_channel is not null) return _channel;

        await _semaphore.WaitAsync(ct);
        try
        {
            if (_channel is null)
            {
                _channel = await connection.CreateChannelAsync(ct);

                await _channel.ExchangeDeclareAsync(
                    MessagingConstants.Exchange, ExchangeType.Topic,
                    durable: true, autoDelete: false, cancellationToken: ct);
            }

            return _channel;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}
