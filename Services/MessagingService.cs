using RabbitMQ.Client;

namespace App.Services;

public class MessagingService : IMessagingService
{
    public ConnectionFactory Factory { get; set; } = default!;
    public IConnection Connection { get; set; } = default!;
    public IChannel Channel { get; set; } = default!;
}
