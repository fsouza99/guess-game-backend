using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class Formula
    {
        public DateTime Creation { get; set; }

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = string.Empty;

        public int ID { get; set; } // PK

        [Required]
        [StringLength(32)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Comment("A JSON string, data template for competitions that use the current formula.")]
        public string DataTemplate { get; set; } = string.Empty;
    }
}
