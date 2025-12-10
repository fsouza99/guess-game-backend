using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models
{
    public class FormulaDto
    {
        public JsonDocument DataTemplate { get; set; } = default!;

        [Required]
        [StringLength(1024)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(32)]
        public string Name { get; set; } = string.Empty;
    }
}
