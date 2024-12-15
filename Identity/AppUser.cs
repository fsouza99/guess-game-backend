using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace App.Identity.Data;

public class AppUser : IdentityUser
{
	[PersonalData]
	[StringLength(32)]
	public string? Name { get; set; }
}
