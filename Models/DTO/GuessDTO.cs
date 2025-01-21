using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models
{
    public class GuessDto
    {
        public JsonDocument Data { get; set; } = default!;
                
        [Required]
        [StringLength(11)]
        public string GameID { get; set; } = default!;

        [StringLength(8)]
        public string? GamePasscode { get; set; }
        
        [Required]
        [StringLength(16)]
        public string Name { get; set; } = default!;
    }
}
