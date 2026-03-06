namespace App.Applications;

public static class GameErrors
{
    public static Error Conflict() => Error.Conflict(
        "Game.Conflict",
        "Game item has been modified since your last read.");

    public static Error Forbidden() => Error.Forbidden(
        "Game.Forbidden",
        "Current user is not allowed to perform this action.");

    public static Error NotFound() => Error.NotFound(
        "Game.NotFound",
        "Game item was not found.");

    public static Error TooEarlySubsDeadline() => Error.Validation(
        "Game.TooEarlySubsDeadline",
        "If given, submission deadline must be at least 5 minutes in future.");

    public static Error CompetitionNotFound() => Error.NotFound(
        "Game.CompetitionNotFound",
        "Referred competition item was not found.");

    public static Error UnfitData() => Error.Conflict(
        "Game.UnfitData",
        "Given JSON data does not match template.");
}
