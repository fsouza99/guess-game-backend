using App.Identity;

namespace App.Models;

public record SimpleGameView
{
    public SimpleGameView(Game game)
    {
        ID = game.ID;
        Competition = new SimpleCompetitionView(game.Competition);
        Name = game.Name;
        Creator = new SimpleAppUserView(game.AppUser);
        Creation = game.Creation;
        MaxGuessCount = game.MaxGuessCount;
        Passcode = game.Passcode;
        SubsDeadline = game.SubsDeadline;
    }

    public string ID { get; }
    public SimpleCompetitionView Competition { get; }
    public string Name { get; }
    public SimpleAppUserView Creator { get; }
    public DateTime Creation { get; }
    public int MaxGuessCount { get; }
    public string? Passcode { get; }
    public DateTime? SubsDeadline { get; }
}
