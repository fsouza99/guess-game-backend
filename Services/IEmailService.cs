namespace App.Services;

public interface IEmailService
{
    public Task SendGameFullEmailAsync(
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick);

    public Task SendGuessCountUpdateEmailAsync(
        int guessCount,
        int maxGuessCount,
        string gameId,
        string gameName,
        string recipient,
        string userNick);
}