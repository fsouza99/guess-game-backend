using System.Text.Json;

namespace App.Models;

public record CompetitionView
{
    public CompetitionView(Competition competition)
    {
        ID = competition.ID;
        FormulaID = competition.FormulaID;
        Name = competition.Name;
        Description = competition.Description;
        Start = competition.Start;
        End = competition.End;
        Data = JsonDocument.Parse(competition.Data);
        Creation = competition.Creation;
        Active = competition.Active;
    }

    public int ID { get; }
    public int FormulaID { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime Start { get; }
    public DateTime End { get; }
    public JsonDocument Data { get; }
    public DateTime Creation { get; }
    public bool Active { get; }
}

