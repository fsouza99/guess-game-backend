using App.Globals;

namespace App.Infrastructure;

public interface IDomainEventsDispatcher
{
    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents);
}
