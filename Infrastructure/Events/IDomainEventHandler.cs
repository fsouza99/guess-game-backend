using Microsoft.Extensions.DependencyInjection;

namespace App.Events;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent);
}
