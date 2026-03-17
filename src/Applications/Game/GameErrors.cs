using App.Globals;

namespace App.Applications;

public static class GameErrors
{
    public static Error CannotDelete() => Error.Forbidden(
        "Game.CannotDelete",
        "Current user is not allowed to delete referred game item.");

    public static Error CannotUpdate() => Error.Forbidden(
        "Game.CannotUpdate",
        "Current user is not allowed to update referred game item.");

    public static Error CompetitionNotFound() => Error.NotFound(
        "Game.CompetitionNotFound",
        "Referred competition item was not found.");

    public static Error NotFound() => Error.NotFound(
        "Game.NotFound",
        "Game item was not found.");

    public static Error TooEarlySubsDeadline() => Error.BadRequest(
        "Game.TooEarlySubsDeadline",
        $"If given, submission deadline must be at least {GameApp.MinSubSpan} minutes in future.");

    public static Error UnfitScoringRules() => Error.BadRequest(
        "Game.UnfitData",
        "Given JSON scoring rules do not match associated template.");

    public static Error UpdateConflict() => Error.Conflict(
        "Game.UpdateConflict",
        "Game item has been modified since your last read.");
}
