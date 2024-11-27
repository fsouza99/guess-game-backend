using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models
{
    public class CompetitionDTO
    {
        public bool Active { get; set; }

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = default!;
        
        public int FormulaID { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; } = default!;
        
        public JsonDocument Data { get; set; } = default!;
    }
}
