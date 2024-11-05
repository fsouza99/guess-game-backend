using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class GuessDTO
    {
        [Required]
        [StringLength(32)]
        public string AuthorName { get; set; } = default!;
                
        [Required]
        public string Data { get; set; } = default!;
        
        public int GameID { get; set; }

        [StringLength(8)]
        public string? GamePasscode { get; set; }
    }
}
