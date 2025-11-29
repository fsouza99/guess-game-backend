using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models
{
    public class CompetitionDto
    {
        public bool Active { get; set; }

        public JsonDocument Data { get; set; } = default!;

        public DateTime End { get; set; }

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = default!;
        
        public int FormulaID { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; } = default!;

        public DateTime Start { get; set; }
    }
}
