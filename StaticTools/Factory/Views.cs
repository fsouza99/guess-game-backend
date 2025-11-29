using App.Identity.Data;
using App.Models;
using System.Text.Json;

namespace App.StaticTools;

public static class ViewFactory
{
    public static CompetitionView Competition(
        Competition competition) => new CompetitionView(
        competition.ID,
        competition.FormulaID,
        competition.Name,
        competition.Description,
        competition.Start,
        competition.End,
        JsonDocument.Parse(competition.Data),
        competition.Creation,
        competition.Active);

    public static SimpleCompetitionView SimpleCompetition(
        Competition competition) => new SimpleCompetitionView(
        competition.ID,
        competition.FormulaID,
        competition.Name,
        competition.Start,
        competition.End);

    public static FormulaView Formula(
        Formula formula) => new FormulaView(
        formula.ID,
        formula.Name,
        formula.Description,
        formula.Creation,
        JsonDocument.Parse(formula.DataTemplate),
        JsonDocument.Parse(formula.ScoringRulesTemplate));

    public static GuessView Guess(Guess guess) => new GuessView(
        guess.Creation,
        JsonDocument.Parse(guess.Data),
        guess.GameID,
        guess.Name,
        guess.Number,
        guess.Score);

    public static GameView Game(Game game) => new GameView(
        game.ID,
        SimpleCompetition(game.Competition),
        game.Name,
        SimpleAppUser(game.AppUser),
        game.Description,
        JsonDocument.Parse(game.ScoringRules),
        game.Creation,
        game.MaxGuessCount,
        game.MaxScore,
        game.Passcode,
        game.SubsDeadline);

    public static SimpleGameView SimpleGame(
        Game game) => new SimpleGameView(
        game.ID,
        SimpleCompetition(game.Competition),
        game.Name,
        SimpleAppUser(game.AppUser),
        game.Creation,
        game.MaxGuessCount,
        game.Passcode,
        game.SubsDeadline);

    public static SimpleAppUserView SimpleAppUser(
        AppUser user) => new SimpleAppUserView(user.Id, user.Nickname);
}

