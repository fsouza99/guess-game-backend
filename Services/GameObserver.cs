using App.Data;
using App.Identity.Data;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class GameObserver : IGameObserver
{
	private readonly AppDbContext _context;
	private readonly IEmailAppMessager _emailMessager;

	public GameObserver(
		AppDbContext context, IEmailAppMessager emailMessager)
	{
		_context = context;
		_emailMessager = emailMessager;
	}

	private bool WatchGuessCount(AppUser gameCreator, Game game, int gameGuessCount)
	{
		if (gameGuessCount == game.MaxGuessCount)
		{	
			_emailMessager.EmailGameFullnessAsync(
	        	game.MaxGuessCount,
	        	game.ID,
	        	game.Name,
	        	gameCreator.Email!,
	        	gameCreator.Nickname!);
			return true;
		}
		if (gameGuessCount == game.MaxGuessCount / 2)
		{	
			_emailMessager.EmailGuessCountAsync(
	        	gameGuessCount,
	        	game.MaxGuessCount,
	        	game.ID,
	        	game.Name,
	        	gameCreator.Email!,
	        	gameCreator.Nickname!);
			return true;
		}
		return false;
	}

	private bool WatchGameCount(AppUser gameCreator, Game game, int creatorGameCount)
	{
		if (creatorGameCount == 1)
		{	
			_emailMessager.EmailFirstGameEverAsync(
	        	game.ID,
	        	game.Name,
	        	gameCreator.Email!,
	        	gameCreator.Nickname!);
			return true;
		}
		return false;
	}

	public async Task WatchAsync(Game game)
	{
		var gameCreator = await _context.AppUser.FindAsync(game.AppUserID)!;
		int creatorGameCount = await _context.Game
			.Where(g => g.AppUserID == game.AppUserID)
			.CountAsync();
		if (WatchGameCount(gameCreator!, game, creatorGameCount))
		{
			return;
		}

		int gameGuessCount = await _context.Guess
			.Where(g => g.GameID == game.ID)
			.CountAsync();
		if (WatchGuessCount(gameCreator!, game, gameGuessCount))
		{
			return;
		}		
	}
}
