namespace App.Models;

public record SimpleCompetitionView(
    int ID,
    int FormulaID,
    string Name,
    DateTime Start,
    DateTime End);

