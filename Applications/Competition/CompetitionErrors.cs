namespace App.Applications;

public static class CompetitionErrors
{
    public static Error UnfitData() => Error.Validation(
        "Competition.UnfitData",
        "Given JSON data does not match template.");

    public static Error Conflict() => Error.Conflict(
        "Competition.Conflict",
        "Competition item has been modified since your last read.");

    public static Error FormulaNotFound() => Error.NotFound(
        "Competition.FormulaNotFound",
        "Referred formula item was not found.");

    public static Error NotFound() => Error.NotFound(
        "Competition.NotFound",
        "Competition item was not found.");
}
