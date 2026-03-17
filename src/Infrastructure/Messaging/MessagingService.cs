using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace App.Infrastructure;

public class MessagingService : IMessagingService
{
    private readonly BasicProperties _properties;
    private readonly ConnectionFactory _factory;
    private readonly IChannel _channel;
    private readonly IConnection _connection;

    public MessagingService(
        ConnectionFactory factory, IConnection connection, IChannel channel)
    {
        _properties = new BasicProperties
        {
            ContentType = "application/json",
            Persistent = true
        };
        _factory = factory;
        _channel = channel;
        _connection = connection;
    }

    public async Task MessageDataAsync(Object data, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        await _channel.BasicPublishAsync(
            basicProperties: _properties,
            body: body,
            exchange: string.Empty,
            mandatory: true,
            routingKey: routingKey);
    }
}

