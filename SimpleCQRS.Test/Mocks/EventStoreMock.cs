using System;
using System.Collections.Generic;
using System.Linq;
using LightMock;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Test.Mocks
{
    public class EventStoreMock : MockBase<IEventStore>, IEventStore
    {
        public IEnumerable<Event> LatestEvents { get; private set; }

        public EventStoreMock()
        {
            _context.Arrange(a => a.SaveEvents(
                The<Guid>.IsAnyValue,
                The<IEnumerable<Event>>.IsAnyValue,
                The<int>.IsAnyValue,
                The<IConcurrencyConflictResolver>.IsAnyValue))
                .Callback<Guid, IEnumerable<Event>, int, IConcurrencyConflictResolver>((a,b,c,d) => LatestEvents = b);
        }

        public void SaveEvents(
            Guid aggregateId, 
            IEnumerable<Event> events, 
            int expectedVersion,
            IConcurrencyConflictResolver resolver)
        {
            _invoker.Invoke(a => a.SaveEvents(aggregateId, events, expectedVersion, resolver));
        }

        public EventStoreMock SeedEvents(
            Guid aggregateId, 
            params Event[] events)
        {
            LatestEvents = events;
            return this;
        }

        public List<Event> GetEventsForAggregate(Guid aggregateId)
        {
            _invoker.Invoke(a => a.GetEventsForAggregate(aggregateId));

            return LatestEvents.ToList();
        }

        public void AssertSaveCalls(int count)
        {
            _context.Assert(a=>
                a.SaveEvents(
                    The<Guid>.IsAnyValue, 
                    The<IEnumerable<Event>>.IsAnyValue, 
                    The<int>.IsAnyValue,
                    The<IConcurrencyConflictResolver>.IsAnyValue), 
                Invoked.Exactly(count));
        }
    }
}
