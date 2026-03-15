using Microsoft.Extensions.DependencyInjection;

namespace App.Globals;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent);
}
