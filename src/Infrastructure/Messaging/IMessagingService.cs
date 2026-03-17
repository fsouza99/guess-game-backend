namespace App.Infrastructure;

public interface IMessagingService
{
    public Task MessageDataAsync(Object data, string routingKey);
}
