using App.Identity.Data;

namespace App.Models;

public record SimpleGameView(
    string ID,
    SimpleCompetitionView Competition,
    string Name,
    SimpleAppUserView Creator,
    DateTime Creation,
    int MaxGuessCount,
    string? Passcode,
    DateTime? SubsDeadline);

