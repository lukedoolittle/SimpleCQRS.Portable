using SimpleCQRS.Domain;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IEventHandler<TEvent>
        where TEvent : Event
    {
        void Handle(TEvent @event);
    }
}
