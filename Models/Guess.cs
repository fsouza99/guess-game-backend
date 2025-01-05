using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    [PrimaryKey(nameof(GameID), nameof(Number))]
    public class Guess
    {
        public DateTime Creation { get; set; }
        
        [Required]
        [Comment("Content of the guess as a JSON string.")]
        public string Data { get; set; } = default!; // JSON    

        [Required]
        [StringLength(11)]
        public string GameID { get; set; } = default!; // FK/PK1
        
        [Required]
        [StringLength(16)]
        public string Name { get; set; } = default!;

        public int Number { get; set; } // PK2

        public int Score { get; set; }
    }
}
