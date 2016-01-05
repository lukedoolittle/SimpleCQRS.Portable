using System.Threading.Tasks;
using LightMock;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Test.Mocks
{
    public class EventPublisherMock : MockBase<IEventPublisher>, IEventPublisher
    {
        private object _lastPublishedObject;

        public Task Publish<T>(T @event) 
            where T : Event
        {
            _lastPublishedObject = @event;
            return _invoker.Invoke(a => a.Publish(@event));
        }

        public void AssertPublishCountAtLeast<T>(int count)
             where T : Event
        {
            _context.Assert(
                a => a.Publish(The<T>.IsAnyValue),
                Invoked.AtLeast(count));
        }

        public void AssertPublishCount<T>(int count)
            where T : Event
        {
            _context.Assert(
                a => a.Publish(The<T>.IsAnyValue),
                Invoked.Exactly(count));
        }

        public T GetLastPublishedObject<T>()
        {
            if (_lastPublishedObject == null)
            {
                return default(T);
            }
            return (T)_lastPublishedObject;
        }
    }
}
