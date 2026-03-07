using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models;

public record CompetitionDto
{
    public bool Active { get; set; }

    [Required]
    public JsonDocument Data { get; set; } = default!;

    [Required]
    public DateTime End { get; set; }

    [Required]
    [StringLength(1024)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int FormulaID { get; set; } // FK

    [Required]
    [StringLength(32)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Start { get; set; }
}

