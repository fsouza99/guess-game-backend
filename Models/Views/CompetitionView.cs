using System.Text.Json;

namespace App.Models;

public record CompetitionView(
    bool Active,
    DateTime Creation,
    JsonDocument Data,
    string Description,
    int FormulaID,
    int ID,
    string Name);

