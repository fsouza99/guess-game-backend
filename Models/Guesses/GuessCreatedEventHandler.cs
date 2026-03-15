using App.Models;
using App.Services;
using System.Threading.Tasks;

namespace App.Events;

public class GuessCreatedEventHandler(
    IGameObserver gameObserver) : IDomainEventHandler<GuessCreatedEvent>
{
    public async Task Handle(GuessCreatedEvent guessEvent)
    {
        await gameObserver.WatchAsync(guessEvent.game);
    }
}
