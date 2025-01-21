using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models
{
    public class GameDto
    {
        public int CompetitionID { get; set; } // FK

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = default!;
        
        [Range(1, 100)]
        public int MaxGuessCount { get; set; }
        
        [Required]
        [StringLength(32)]
        public string Name { get; set; } = default!;
        
        [StringLength(8)]
        public string? Passcode { get; set; }

        public JsonDocument ScoringRules { get; set; } = default!;
        
        public DateTime? SubsDeadline { get; set; }
    }
}
