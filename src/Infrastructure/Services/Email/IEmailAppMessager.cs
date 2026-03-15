namespace App.Infrastructure;

/* Pushes messages to email application requesting it to send user notifications. */
public interface IEmailAppMessager
{
    public Task EmailGameFullnessAsync(
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick);

    public Task EmailFirstGameEverAsync(
        string gameId,
        string gameName,
        string recipient,
        string userNick);

    public Task EmailGuessCountAsync(
        int guessCount,
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick);
}