namespace App.Applications;

public static class GuessErrors
{
    public static Error CannotDelete() => Error.Forbidden(
        "Guess.CannotDelete",
        "Current user is not allowed to delete referred guess item.");

    public static Error DeadlinePassed() => Error.Conflict(
        "Guess.DeadlinePassed",
        "Game's guess submission deadline has passed.");

    public static Error GameNotFound() => Error.NotFound(
        "Guess.GameNotFound",
        "Referred game item was not found.");

    public static Error NotFound() => Error.NotFound(
        "Guess.NotFound",
        "Guess item was not found.");

    public static Error TooManyGuesses() => Error.Conflict(
        "Guess.TooManyGuesses",
        "Referred game has reached its maximum amount of registered guesses.");

    public static Error UnfitData() => Error.BadRequest(
        "Guess.UnfitData",
        "Given JSON data does not match template.");

    public static Error WrongPasscode() => Error.Forbidden(
        "Guess.WrongPasscode",
        "Submission passcode is incorrect.");
}
