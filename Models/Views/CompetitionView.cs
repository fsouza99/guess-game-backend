using System.Text.Json;

namespace App.Models;

public record CompetitionView(
    int ID,
    int FormulaID,
    string Name,
    string Description,
    DateTime Start,
    DateTime End,
    JsonDocument Data,
    DateTime Creation,
    bool Active);

