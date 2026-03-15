using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Models;

[Index(nameof(Nickname), IsUnique = true)]
public class AppUser : IdentityUser
{
    [Required]
    [StringLength(16)]
    public string Nickname { get; set; } = string.Empty;
}
