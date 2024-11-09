using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    [PrimaryKey(nameof(GameID), nameof(Number))]
    public class Guess
    {
        [Required]
        [StringLength(32)]
        public string AuthorName { get; set; } = default!;
        
        public DateTime Creation { get; set; }

        [Required]
        [Comment("Content of the guess as a JSON string.")]
        public string Data { get; set; } = default!; // JSON    
        
        public int GameID { get; set; } // FK/PK1

        public int Number { get; set; } // PK2

        public int Score { get; set; }
    }
}
