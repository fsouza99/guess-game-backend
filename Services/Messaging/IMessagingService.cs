namespace App.Services;

public interface IMessagingService
{
    public Task MessageDataAsync(Object data, string routingKey);
}
