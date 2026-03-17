using App.Applications;
using App.Globals;
using App.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure;

public class AppDbContext : IdentityDbContext<AppUser>, IAppDbContext
{
    private readonly IDomainEventsDispatcher _eventDispatcher;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IDomainEventsDispatcher eventDispatcher) : base(options)
    {
        _eventDispatcher = eventDispatcher;
    }

    public DbSet<AppUser> AppUser { get; set; } = default!;
    public DbSet<Formula> Formula { get; set; } = default!;
    public DbSet<Competition> Competition { get; set; } = default!;
    public DbSet<Game> Game { get; set; } = default!;
    public DbSet<Guess> Guess { get; set; } = default!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events after transactions.
        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
                {
                    List<IDomainEvent> domainEvents = entity.DomainEvents;
                    entity.ClearDomainEvents();
                    return domainEvents;
                })
            .ToList();

        await _eventDispatcher.DispatchAsync(domainEvents);
    }
}
