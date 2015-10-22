using SimpleCQRS.Domain;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IEventPublisher
    {
        void Publish<T>(T @event) 
            where T : Event;
    }
}