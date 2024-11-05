using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class FormulaDTO
    {
        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = default!;
        
        [Required]
        [StringLength(32)]
        public string Name { get; set; } = default!;
        
        [Required]
        public string DataTemplate { get; set; } = default!;

        [Required]
        public string ScoringRulesTemplate { get; set; } = default!;
    }
}
