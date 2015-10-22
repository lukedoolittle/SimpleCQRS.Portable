using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Exceptions;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    public class EventStore : IEventStore
    {
        private readonly static object _saveLock = new object();
        private readonly IEventPublisher _publisher;
        private readonly IDatabase<EventDescriptors> _database;

        public EventStore(
            IEventPublisher publisher,
            IDatabase<EventDescriptors> database)
        {
            _publisher = publisher;
            _database = database;
        }
       
        public void SaveEvents(
            Guid aggregateId, 
            IEnumerable<Event> events, 
            int expectedVersion,
            IConcurrencyConflictResolver conflictResolver)
        {
            lock (_saveLock)
            {
                var eventDescriptors =
                    _database.Get(aggregateId.ToString());
                var actualVersion = expectedVersion;

                if (eventDescriptors == null)
                {
                    eventDescriptors = new EventDescriptors {Id = aggregateId};
                    _database.Put(eventDescriptors);
                }
                else
                {
                    actualVersion = GetActualVersion(
                        eventDescriptors,
                        expectedVersion,
                        events,
                        conflictResolver);
                }

                var i = actualVersion;
                foreach (var @event in events)
                {
                    i++;
                    @event.Version = i;
                    @event.AggregateId = aggregateId;
                    @event.Id = Guid.NewGuid();
                    eventDescriptors.Add(
                        new EventDescriptor(aggregateId, @event, i));
                    _publisher.Publish(@event);
                }

                _database.Update(eventDescriptors);
            }
        }

        private int GetActualVersion(
            EventDescriptors eventDescriptors, 
            int expectedVersion,
            IEnumerable<Event> events,
            IConcurrencyConflictResolver conflictResolver)
        {
            var actualVersion = eventDescriptors[eventDescriptors.Count() - 1].Version;

            if (actualVersion != expectedVersion && expectedVersion != -1)
            {
                var potentialConflictingEvents =
                    eventDescriptors
                    .Where(e => e.Version > expectedVersion)
                    .Select(e => e.EventData);
                if (AnyEventsConflict(events, potentialConflictingEvents, conflictResolver))
                {
                    throw new EventStoreConcurrencyException();
                }
                else
                {
                    return actualVersion;
                }
            }

            return expectedVersion;
        }

        private bool AnyEventsConflict(
            IEnumerable<Event> events,
            IEnumerable<Event> conflictingEvents,
            IConcurrencyConflictResolver conflictResolver)
        {
            if (conflictResolver == null)
            {
                return true;
            }

            var eventTypes = events.Select(e => e.GetType());
            var conflictingEventTypes = conflictingEvents.Select(e => e.GetType());
            return eventTypes.Any(eventType => 
                conflictResolver.ConflictsWith(
                    eventType, 
                    conflictingEventTypes));
        }

        public List<Event> GetEventsForAggregate(Guid aggregateId)
        {
            var eventDescriptors = _database.Get(aggregateId.ToString());

            if (eventDescriptors == null)
            {
                throw new AggregateNotFoundException();
            }

            return eventDescriptors.Select(desc => desc.EventData).ToList();
        }
    }
}