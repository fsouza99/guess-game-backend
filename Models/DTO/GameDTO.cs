using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class GameDTO
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

        [Required]
        public string ScoringRules { get; set; } = default!;
        
        public DateTime? SubsDeadline { get; set; }
    }
}
