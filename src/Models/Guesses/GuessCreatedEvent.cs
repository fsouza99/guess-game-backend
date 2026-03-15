using App.Globals;

namespace App.Models;

public sealed record GuessCreatedEvent(Game game, Guess guess) : IDomainEvent;
