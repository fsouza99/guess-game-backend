using App.Globals;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure;

public class GuessCreatedEventHandler(
    AppDbContext context,
    IEmailAppMessenger emailMessenger) : IDomainEventHandler<GuessCreatedEvent>
{
    public async Task Handle(GuessCreatedEvent ev)
    {
        // Load associated "AppUser" (note the current context is not the one that loaded the game).
        ev.game.AppUser = (await context.AppUser.FirstOrDefaultAsync(
            a => a.Id == ev.game.AppUserID))!;

        if (await CheckGameGuessCount(ev.game))
        {
            return;
        }

        await CheckAppUserGameCount(ev.game);
    }

    private async Task<bool> CheckGameGuessCount(Game game)
    {
        int count = await context.Guess
            .Where(g => g.GameID == game.ID)
            .CountAsync();

        if (count == game.MaxGuessCount)
        {
            await emailMessenger.EmailGameFullnessAsync(
                game.MaxGuessCount, game.ID, game.Name, game.AppUser.Email!, game.AppUser.Nickname);
            return true;
        }

        if (count == game.MaxGuessCount / 2)
        {
            await emailMessenger.EmailGuessCountAsync(
                count,
                game.MaxGuessCount,
                game.ID,
                game.Name,
                game.AppUser.Email!,
                game.AppUser.Nickname);
            return true;
        }

        return false;
    }

    private async Task<bool> CheckAppUserGameCount(Game game)
    {
        bool firstAppUserGame = await context.Game
            .Where(g => g.AppUserID == game.AppUserID)
            .AnyAsync();

        if (firstAppUserGame)
        {   
            await emailMessenger.EmailFirstGameEverAsync(
                game.ID, game.Name, game.AppUser.Email!, game.AppUser.Nickname);
            return true;
        }

        return false;
    }
}
