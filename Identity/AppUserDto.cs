using System.ComponentModel.DataAnnotations;

namespace App.Identity.Data;

public class AppUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    public string Password { get; set; } = string.Empty;
}
