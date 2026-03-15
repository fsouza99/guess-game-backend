using System.Text.Json;

namespace App.Models;

public record GameView
{
    public GameView(Game game)
    {
        ID = game.ID;
        Competition = new SimpleCompetitionView(game.Competition);
        Name = game.Name;
        Creator = new SimpleAppUserView(game.AppUser);
        Description = game.Description;
        ScoringRules = JsonDocument.Parse(game.ScoringRules);
        Creation = game.Creation;
        MaxGuessCount = game.MaxGuessCount;
        MaxScore = game.MaxScore;
        Passcode = game.Passcode;
        SubsDeadline = game.SubsDeadline;
    }

    public string ID { get; }
    public SimpleCompetitionView Competition { get; }
    public string Name { get; }
    public SimpleAppUserView Creator { get; }
    public string Description { get; }
    public JsonDocument ScoringRules { get; }
    public DateTime Creation { get; }
    public int MaxGuessCount { get; }
    public int MaxScore { get; }
    public string? Passcode { get; }
    public DateTime? SubsDeadline { get; }
}
