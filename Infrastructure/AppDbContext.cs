using App.Identity;
using App.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<AppUser> AppUser { get; set; } = default!;
        public DbSet<Competition> Competition { get; set; } = default!;
        public DbSet<Formula> Formula { get; set; } = default!;
        public DbSet<Game> Game { get; set; } = default!;
        public DbSet<Guess> Guess { get; set; } = default!;
    }
}
