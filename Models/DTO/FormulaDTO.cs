using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
        
        public JsonDocument DataTemplate { get; set; } = default!;

        public JsonDocument ScoringRulesTemplate { get; set; } = default!;
    }
}
