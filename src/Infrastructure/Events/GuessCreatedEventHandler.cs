using App.Globals;
using App.Models;
using System.Threading.Tasks;

namespace App.Infrastructure;

public class GuessCreatedEventHandler(
    IGameObserver gameObserver) : IDomainEventHandler<GuessCreatedEvent>
{
    public async Task Handle(GuessCreatedEvent guessEvent)
    {
        await gameObserver.WatchAsync(guessEvent.game);
    }
}
