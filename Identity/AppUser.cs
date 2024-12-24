using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Identity.Data;

public class AppUser : IdentityUser
{
	// Identity usernames are used for authentication and cannot be so easily changed.
	public string? Nickname { get; set; }
}
