namespace App.Applications;

public static class CompetitionErrors
{
    public static Error FormulaNotFound() => Error.NotFound(
        "Competition.FormulaNotFound",
        "Referred formula item was not found.");

    public static Error NotFound() => Error.NotFound(
        "Competition.NotFound",
        "Competition item was not found.");

    public static Error UnfitData() => Error.BadRequest(
        "Competition.UnfitData",
        "Given JSON data does not match associated template.");

    public static Error UpdateConflict() => Error.Conflict(
        "Competition.UpdateConflict",
        "Competition item has been modified since your last read.");
}
