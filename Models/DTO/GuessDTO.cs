using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Models
{
    public class GuessDTO
    {
        [Required]
        [StringLength(32)]
        public string AuthorName { get; set; } = default!;
                
        public JsonDocument Data { get; set; } = default!;
        
        public int GameID { get; set; }

        [StringLength(8)]
        public string? GamePasscode { get; set; }
    }
}
