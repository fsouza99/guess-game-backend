using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class Formula
    {
        public DateTime Creation { get; set; }

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = default!;
        
        public int ID { get; set; } // PK

        [Required]
        [StringLength(32)]
        public string Name { get; set; } = default!;

        // JSON Templates for reference

        [Required]
        [Comment("A JSON string template for guesses and real data about competitions that use this format.")]
        public string DataTemplate { get; set; } = default!;

        [Required]
        [Comment("A JSON string template for criteria used to evaluate guess data.")]
        public string ScoringRulesTemplate { get; set; } = default!;
    }
}
