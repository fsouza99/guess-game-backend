using System.Text.Json;

namespace App.Models;

public record FormulaView
{
    public FormulaView(Formula formula)
    {
        ID = formula.ID;
        Name = formula.Name;
        Description = formula.Description;
        Creation = formula.Creation;
        DataTemplate = JsonDocument.Parse(formula.DataTemplate);
    }

    public int ID { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime Creation { get; }
    public JsonDocument DataTemplate { get; }
}
