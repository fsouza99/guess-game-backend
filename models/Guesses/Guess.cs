using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models;

[PrimaryKey(nameof(GameID), nameof(Number))]
public class Guess
{
    public DateTime Creation { get; set; }

    [Required]
    [Comment("Content of the guess as a JSON string.")]
    public string Data { get; set; } = string.Empty;

    [Required]
    [StringLength(11)]
    public string GameID { get; set; } = string.Empty;

    [Required]
    [StringLength(16)]
    public string Name { get; set; } = string.Empty;

    public int Number { get; set; }

    public int Score { get; set; }
}
