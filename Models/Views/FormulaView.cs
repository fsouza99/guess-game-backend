using System.Text.Json;

namespace App.Models;

public record FormulaView(
    int ID,
    string Name,
    string Description,
    DateTime Creation,
    JsonDocument DataTemplate);

