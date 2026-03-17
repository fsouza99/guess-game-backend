using App.Globals;
using App.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

namespace App.Applications;

public interface IAppDbContext
{
    DbSet<AppUser> AppUser { get; set; }
    DbSet<Formula> Formula { get; set; }
    DbSet<Competition> Competition { get; set; }
    DbSet<Game> Game { get; set; }
    DbSet<Guess> Guess { get; set; }

    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
