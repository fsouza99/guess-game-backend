using System.Text.Json;

namespace App.Models;

public record GuessView
{
    public GuessView(Guess guess)
    {
        Creation = guess.Creation;
        Data = JsonDocument.Parse(guess.Data);
        GameID = guess.GameID;
        Name = guess.Name;
        Number = guess.Number;
        Score = guess.Score;
    }

    public DateTime Creation { get; }
    public JsonDocument Data { get; }
    public string GameID { get; }
    public string Name { get; }
    public int Number { get; }
    public int Score { get; }
}

