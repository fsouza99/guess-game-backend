using App.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class Game
{
    [Required]
    public string AppUserID { get; set; } = string.Empty;

    public int CompetitionID { get; set; }

    public DateTime Creation { get; set; }

    [Required]
    [StringLength(256)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength=11)]
    public string ID { get; set; } = string.Empty;

    [Range(1, 100)]
    public int MaxGuessCount { get; set; }

    [Comment("The maximum possible score of a guess on this game.")]
    public int MaxScore { get; set; }

    [Comment("Avoid giving a guess the same number as another previously deleted one.")]
    public int NextGuessNumber { get; set; } = 1;

    [Required]
    [StringLength(32)]
    public string Name { get; set; } = string.Empty;

    [StringLength(8)]
    [Comment("A passcode might be necessary to enter the game.")]
    public string? Passcode { get; set; }

    [Required]
    [Comment("Scoring rules as a JSON string.")]
    public string ScoringRules { get; set; } = string.Empty;

    [Comment("If set, new guesses will be accepted until this datetime.")]
    public DateTime? SubsDeadline { get; set; }

    // Navigation

    public AppUser AppUser { get; set; } = default!;

    public Competition Competition { get; set; } = default!;

    public ICollection<Guess> Guesses { get; set; } = default!;
}
