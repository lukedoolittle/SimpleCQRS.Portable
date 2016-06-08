using System.Threading.Tasks;
using SimpleCQRS.Domain;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) 
            where T : Event;
    }
}