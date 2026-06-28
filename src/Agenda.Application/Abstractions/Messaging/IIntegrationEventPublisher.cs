namespace Agenda.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken = default);
}
