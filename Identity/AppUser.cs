using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Identity.Data;

public class AppUser : IdentityUser
{
	public string? Nickname { get; set; }
}
