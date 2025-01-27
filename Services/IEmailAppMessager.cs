namespace App.Services;

/* Pushes messages to email application requesting it to send user notifications. */
public interface IEmailAppMessager
{
    public Task MessageGameFullnessAsync(
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick);

    public Task MessageFirstGameEverAsync(
        string gameId,
        string gameName,
        string recipient,
        string userNick);

    public Task MessageGuessCountAsync(
        int guessCount,
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick);
}