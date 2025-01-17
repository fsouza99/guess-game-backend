using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace App.Services;

public class EmailService : IEmailService
{
    private readonly BasicProperties _properties;
    private readonly IMessagingService _messagingService;
    private readonly string _routingKey;

    public EmailService(IMessagingService messageService, IConfiguration config)
    {
        _messagingService = messageService;
        _routingKey = config["Messaging:EmailQueue"]!;
        _properties = new BasicProperties
        {
            ContentType = "application/json",
            Persistent = true
        };
    }

    private async Task SendEmailAsync(Object data)
    {
    	var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
    	await _messagingService.Channel.BasicPublishAsync(
            basicProperties: _properties,
            body: body,
            exchange: string.Empty,
            mandatory: true,
            routingKey: _routingKey);
    	return;
    }

    public async Task SendGameFullEmailAsync(
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick)
    {
        var data = new
        {
            GameID = gameId,
            GameName = gameName,
            MaxGuessCount = maxGuessCount,
            Recipient = recipient,
            Template = "GFL",
            UserNick = userNick
        };
        await SendEmailAsync(data);
        return;
    }

    public async Task SendGuessCountUpdateEmailAsync(
        int guessCount,
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick)
    {
    	var data = new
    	{
			GameID = gameId,
			GameName = gameName,
			GuessCount = guessCount,
			MaxGuessCount = maxGuessCount,
    		Recipient = recipient,
            Template = "GCU",
    		UserNick = userNick
    	};
        await SendEmailAsync(data);
        return;
    }
}
