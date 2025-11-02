using System.Text.Json;

namespace App.Models;

public record GuessView(
    DateTime Creation,
    JsonDocument Data,
    string GameID,
    string Name,
    int Number,
    int Score);

