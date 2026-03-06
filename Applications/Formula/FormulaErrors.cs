namespace App.Applications;

public static class FormulaErrors
{
    public static Error BadTemplate() => Error.Validation(
        "Formula.BadTemplate",
        "Given JSON template is unacceptable.");

    public static Error Conflict() => Error.Conflict(
        "Formula.Conflict",
        "Formula item has been modified since your last read.");

    public static Error NotFound() => Error.NotFound(
        "Formula.NotFound",
        "Formula item was not found.");
}
