namespace App.Applications;

public static class GuessErrors
{
    public static Error DeadlinePassed() => Error.Conflict(
        "Guess.DeadlinePassed",
        "Game's guess submission deadline has passed.");

    public static Error Forbidden() => Error.Forbidden(
        "Guess.Forbidden",
        "Current user is not allowed to perform this action.");

    public static Error GameNotFound() => Error.NotFound(
        "Guess.GameNotFound",
        "Referred game item was not found.");

    public static Error NotFound() => Error.NotFound(
        "Guess.NotFound",
        "Guess item was not found.");

    public static Error TooManyGuesses() => Error.Conflict(
        "Guess.TooManyGuesses",
        "Referred game has reached its maximum amount of registered guesses.");

    public static Error UnfitData() => Error.Validation(
        "Guess.UnfitData",
        "Given JSON data does not match template.");

    public static Error WrongPassword() => Error.Unauthorized(
        "Guess.WrongPassword",
        "Submission password is incorrect.");
}
