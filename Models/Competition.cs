using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class Competition
    {
        public bool Active { get; set; }

        public DateTime Creation { get; set; }

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = default!;
        
        public int FormulaID { get; set; }

        public int ID { get; set; } // PK

        [Required]
        [StringLength(32)]
        public string Name { get; set; } = default!;
        
        [Required]
        [Comment("Real world data as a JSON string.")]
        public string Data { get; set; } = default!; // JSON

        // Navigation

        public Formula Formula { get; set; } = default!;
    }
}
