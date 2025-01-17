using RabbitMQ.Client;

namespace App.Services;

public interface IMessagingService
{
    public ConnectionFactory Factory { get; set; }
    public IConnection Connection { get; set; }
    public IChannel Channel { get; set; }
}
