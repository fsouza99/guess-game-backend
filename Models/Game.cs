using App.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class Game
    {
        [Required]
        public string AppUserID { get; set; } = default!; // FK

        public int CompetitionID { get; set; } // FK

        public DateTime Creation { get; set; }
        
        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = default!;
        
        public int ID { get; set; } // PK
        
        [Range(1, 100)]
        public int MaxGuessCount { get; set; }
        
        [Required]
        [StringLength(32)]
        public string Name { get; set; } = default!;

        [StringLength(8)]
        [Comment("A passcode might be necessary to enter the competition.")]
        public string? Passcode { get; set; }

        [Required]
        [Comment("Scoring rules as a JSON string.")]
        public string ScoringRules { get; set; } = default!; // JSON
        
        [Comment("People can enter the competition until this datetime.")]
        public DateTime? SubsDeadline { get; set; }

        // Navigation

        public AppUser AppUser { get; set; } = default!;

        public Competition Competition { get; set; } = default!;

        public ICollection<Guess> Guesses { get; set; } = default!;
    }
}
