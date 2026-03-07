using RabbitMQ.Client;

namespace App.Services;

public static class MessagingServiceFactory
{
    public static async Task<MessagingService> Create(string hostName)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        var service = new MessagingService(factory, connection, channel);
        
        return service;
    }

    public static EmptyMessagingService CreateEmpty()
    {
        return new EmptyMessagingService();
    }
}