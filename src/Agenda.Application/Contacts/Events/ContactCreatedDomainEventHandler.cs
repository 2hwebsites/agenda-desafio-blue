using Agenda.Application.Abstractions.Messaging;
using Agenda.Contracts;
using MediatR;

namespace Agenda.Application.Contacts.Events;

public sealed class ContactCreatedDomainEventHandler(IIntegrationEventPublisher publisher)
    : INotificationHandler<ContactCreatedDomainEvent>
{
    public Task Handle(ContactCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ContactCreatedIntegrationEvent(
            notification.Id,
            notification.Name,
            notification.Email,
            notification.CreatedAt);

        return publisher.PublishAsync(integrationEvent, MessagingConstants.RoutingKey, cancellationToken);
    }
}
