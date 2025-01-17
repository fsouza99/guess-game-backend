using RabbitMQ.Client;

namespace App.Services;

public static class MessagingServiceFactory
{
    public static async Task<MessagingService> Create(string hostName)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        return new MessagingService
        {
            Factory = factory,
            Connection = connection,
            Channel = channel
        };
    }
}