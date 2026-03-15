namespace App.Models;

public record SimpleCompetitionView
{
    public SimpleCompetitionView(Competition competition)
    {
        ID = competition.ID;
        FormulaID = competition.FormulaID;
        Name = competition.Name;
        Start = competition.Start;
        End = competition.End;
    }

    public int ID { get; }
    public int FormulaID { get; }
    public string Name { get; }
    public DateTime Start { get; }
    public DateTime End { get; }
}

