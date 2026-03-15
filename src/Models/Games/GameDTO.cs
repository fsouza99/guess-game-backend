using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models;

public record GameDto
{
    [Required]
    public int CompetitionID { get; set; } // FK

    [Required]
    [StringLength(256)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int MaxGuessCount { get; set; }

    [Required]
    [StringLength(32)]
    public string Name { get; set; } = string.Empty;

    [StringLength(8)]
    public string? Passcode { get; set; }

    [Required]
    public JsonDocument ScoringRules { get; set; } = default!;

    public DateTime? SubsDeadline { get; set; }
}

