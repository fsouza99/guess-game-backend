using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Identity.Data;

[Index(nameof(Nickname), IsUnique = true)]
public class AppUser : IdentityUser
{
	// Identity usernames are used for authentication and cannot be so easily changed.
	[StringLength(16)]
	public string Nickname { get; set; } = string.Empty;
}
