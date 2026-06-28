namespace Agenda.Contracts;

public static class MessagingConstants
{
    public const string Exchange = "agenda.events";
    public const string RoutingKey = "contact.created";
    public const string Queue = "contact.created.welcome-email";
}
