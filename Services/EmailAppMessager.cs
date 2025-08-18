namespace App.Services;

public class EmailAppMessager : IEmailAppMessager
{
    private readonly IMessagingService _messagingService;
    private readonly string _routingKey;

    public EmailAppMessager(IMessagingService messageService, IConfiguration config)
    {
        _messagingService = messageService;
        _routingKey = config["Messaging:EmailQueue"]!;
    }

    public async Task EmailGameFullnessAsync(
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
        await _messagingService.MessageDataAsync(data, _routingKey);
    }

    public async Task EmailFirstGameEverAsync(
        string gameId,
        string gameName,
        string recipient,
        string userNick)
    {
        var data = new
        {
            GameID = gameId,
            GameName = gameName,
            Recipient = recipient,
            Template = "FGE",
            UserNick = userNick
        };
        await _messagingService.MessageDataAsync(data, _routingKey);
    }

    public async Task EmailGuessCountAsync(
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
        await _messagingService.MessageDataAsync(data, _routingKey);
    }
}
