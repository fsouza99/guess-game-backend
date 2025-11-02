using System.Text.Json;

namespace App.Models;

public record FormulaView(
    DateTime Creation,
    JsonDocument DataTemplate,
    string Description,
    int ID,
    string Name,
    JsonDocument ScoringRulesTemplate);

