using App.Globals;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace App.Infrastructure;

internal sealed class DomainEventsDispatcher(
    IServiceProvider serviceProvider) : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDict = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDict = new();

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            Type domainEventType = domainEvent.GetType();
            Type handlerType = HandlerTypeDict.GetOrAdd(
                domainEventType, et => typeof(IDomainEventHandler<>).MakeGenericType(et));

            IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

            foreach (object? handler in handlers)
            {
                if (handler is not null)
                {
                    var handlerWrapper = HandlerWrapper.Create(handler, domainEventType);
                    await handlerWrapper.Handle(domainEvent);
                }
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(IDomainEvent domainEvent);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            Type wrapperType = WrapperTypeDict.GetOrAdd(
                domainEventType, et => typeof(HandlerWrapper<>).MakeGenericType(et));

            return (HandlerWrapper) Activator.CreateInstance(wrapperType, handler)!;
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>) handler;

        public override async Task Handle(IDomainEvent domainEvent)
        {
            await _handler.Handle((T) domainEvent);
        }
    }
}
