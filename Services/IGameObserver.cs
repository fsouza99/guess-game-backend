using App.Models;

namespace App.Services;

/*! Examines a Game object looking for interesting events that might trigger user notifications. */
public interface IGameObserver
{
	public Task WatchAsync(Game game);
}