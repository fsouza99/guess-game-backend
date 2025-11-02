using System.Text.Json;

namespace App.Models;

public record GameView(
    int CompetitionID,
    DateTime Creation,
    string CreatorID,
    string CreatorNick,
    string Description,
    string ID,
    int MaxGuessCount,
    int MaxScore,
    string Name,
    string? Passcode,
    JsonDocument ScoringRules,
    DateTime? SubsDeadline);

