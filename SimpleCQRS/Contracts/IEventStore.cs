using System;
using System.Collections.Generic;
using SimpleCQRS.Domain;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IEventStore
    {
        void SaveEvents(
            Guid aggregateId, 
            IEnumerable<Event> events, 
            int expectedVersion,
            IConcurrencyConflictResolver conflictResolver);

        List<Event> GetEventsForAggregate(Guid aggregateId);
    }
}
