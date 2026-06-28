using Agenda.Application.Abstractions.Messaging;

namespace Agenda.Infrastructure.Messaging;

internal sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
