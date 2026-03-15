using App.Globals;

namespace App.Applications;

public static class FormulaErrors
{
    public static Error BadDataTemplate() => Error.BadRequest(
        "Formula.BadDataTemplate",
        "Given JSON template is unacceptable.");

    public static Error UpdateConflict() => Error.Conflict(
        "Formula.UpdateConflict",
        "Formula item has been modified since your last read.");

    public static Error NotFound() => Error.NotFound(
        "Formula.NotFound",
        "Formula item was not found.");
}
