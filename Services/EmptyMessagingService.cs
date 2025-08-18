namespace App.Services;

public class EmptyMessagingService : IMessagingService
{
    public async Task MessageDataAsync(Object data, string routingKey)
    {
        await Task.Delay(200);
    }
}
