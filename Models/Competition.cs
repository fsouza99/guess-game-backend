using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class Competition
    {
        public bool Active { get; set; }

        public DateTime Creation { get; set; }

        [Required]
        [Comment("Real world data as a JSON string.")]
        public string Data { get; set; } = string.Empty;

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = string.Empty;

        public DateTime End { get; set; }

        public int FormulaID { get; set; }

        public int ID { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; } = string.Empty;

        public DateTime Start { get; set; }

        // Navigation

        public Formula Formula { get; set; } = default!;
    }
}
