using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Exceptions;
using SimpleCQRS.Framework.Contracts;
using SimpleCQRS.Infrastructure;
using SimpleCQRS.Test.Eventing;
using SimpleCQRS.Test.Eventing.EventConstraintOne;
using SimpleCQRS.Test.Mocks;
using Xunit;

namespace SimpleCQRS.Test
{
    public class EventStoreTests
    {
        [Fact]
        public void AddingNoEventsToEmptyDatabaseCreatesEventDescriptors()
        {
            var aggregateId = Guid.NewGuid();
            var expectedVersion = -1;
            IConcurrencyConflictResolver conflictResolver = null;
            var events = new List<Event>();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            eventStore.SaveEvents(
                aggregateId, 
                events, 
                expectedVersion, 
                conflictResolver);

            var actual = databaseMock.Get(aggregateId.ToString());

            Assert.NotNull(actual);
            Assert.Equal(0, actual.Count());
            Assert.Null(publisherMock.GetLastPublishedObject<Event>());
        }

        [Fact]
        public void AddingEventsToEmptyDatabaseCreatesProperEventsInDatabase()
        {
            var aggregateId = Guid.NewGuid();
            var expectedVersion = -1;
            IConcurrencyConflictResolver conflictResolver = null;
            var events = new List<Event>
            {
                new Event(),
                new Event()
            };

            var databaseMock = new DatabaseMock<EventDescriptors>();
            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            eventStore.SaveEvents(
                aggregateId,
                events,
                expectedVersion,
                conflictResolver);

            var actual = databaseMock.Get(aggregateId.ToString());

            Assert.NotNull(actual);
            Assert.Equal(events.Count, actual.Count());
            publisherMock.AssertPublishCount<Event>(events.Count());
        }

        [Fact]
        public void AddingEventsWithProperExpectedVersionToExistingEventDescriptorsPopulatesDatabase()
        {
            var aggregateId = Guid.NewGuid();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var existingEvents = new EventDescriptors{ Id = aggregateId };
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 0));
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 1));
            databaseMock.Put(existingEvents);
            var initialCount = existingEvents.Count();

            var expectedVersion = existingEvents.Count() - 1;
            IConcurrencyConflictResolver conflictResolver = null;
            var newEvents = new List<Event>
            {
                new Event(),
                new Event()
            };
            
            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            eventStore.SaveEvents(
                aggregateId,
                newEvents,
                expectedVersion,
                conflictResolver);

            var actual = databaseMock.Get(aggregateId.ToString());

            Assert.NotNull(actual);
            Assert.Equal(newEvents.Count + initialCount, actual.Count());
            publisherMock.AssertPublishCount<Event>(newEvents.Count());
        }

        [Fact]
        public void AddingEventsWithImproperExpectedVersionAndNullConflictResolverExpectException()
        {
            var aggregateId = Guid.NewGuid();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var existingEvents = new EventDescriptors { Id = aggregateId };
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 0));
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 1));
            databaseMock.Put(existingEvents);

            var expectedVersion = existingEvents.Count() - 2;
            IConcurrencyConflictResolver conflictResolver = null;
            var newEvents = new List<Event>
            {
                new Event(),
                new Event()
            };

            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            Assert.Throws<EventStoreConcurrencyException>(() => 
                eventStore.SaveEvents(
                    aggregateId,
                    newEvents,
                    expectedVersion,
                    conflictResolver));
        }

        [Fact]
        public void AddingImproperlyVersionedEventsWithNoConflictResolutionExpectException()
        {
            var aggregateId = Guid.NewGuid();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var existingEvents = new EventDescriptors { Id = aggregateId };
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event1(), 0));
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event1(), 1));
            databaseMock.Put(existingEvents);

            var expectedVersion = existingEvents.Count() - 2;
            IConcurrencyConflictResolver conflictResolver = new ConcurrencyConflictResolver();
            conflictResolver.RegisterConflictList(typeof(Event1), new List<Type> {typeof(Event1)});
            var newEvents = new List<Event>
            {
                new Event1(),
                new Event1()
            };

            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            Assert.Throws<EventStoreConcurrencyException>(() =>
                eventStore.SaveEvents(
                    aggregateId,
                    newEvents,
                    expectedVersion,
                    conflictResolver));
        }

        [Fact]
        public void AddingImproperlyVersionedEventsWithConflictResolutionPopulatesDatabase()
        {
            var aggregateId = Guid.NewGuid();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var existingEvents = new EventDescriptors { Id = aggregateId };
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 0));
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 1));
            databaseMock.Put(existingEvents);
            var initialCount = existingEvents.Count();

            var expectedVersion = existingEvents.Count() - 2;
            IConcurrencyConflictResolver conflictResolver = new ConcurrencyConflictResolver();
            conflictResolver.RegisterConflictList(typeof(Event1), new List<Type>());
            var newEvents = new List<Event>
            {
                new Event1(),
                new Event1()
            };

            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            eventStore.SaveEvents(
                aggregateId,
                newEvents,
                expectedVersion,
                conflictResolver);

            var actual = databaseMock.Get(aggregateId.ToString());

            Assert.NotNull(actual);
            Assert.Equal(newEvents.Count + initialCount, actual.Count());
            publisherMock.AssertPublishCount<Event>(newEvents.Count());
        }

        [Fact]
        public async void AddingImproperlyVersionedEventsConcurrentlyWithConflictResolutionPopulatesDatabase()
        {
            var aggregateId = Guid.NewGuid();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var existingEvents = new EventDescriptors {Id = aggregateId};
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 0));
            existingEvents.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 1));
            databaseMock.Put(existingEvents);
            var initialCount = existingEvents.Count();

            var expectedVersion = 0;
            IConcurrencyConflictResolver conflictResolver = new ConcurrencyConflictResolver();
            conflictResolver.RegisterConflictList(typeof (Event2<EventConstraint1>), new List<Type>());
            conflictResolver.RegisterConflictList(typeof (Event2<EventConstraintAnother1>), new List<Type>());
            var newEventsSource1 = new List<Event>
            {
                new Event2<EventConstraint1>()
            };
            var newEventsSource2 = new List<Event>
            {
                new Event2<EventConstraintAnother1>()
            };

            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            var task1 = Task.Run(() =>
                eventStore.SaveEvents(
                    aggregateId,
                    newEventsSource1,
                    expectedVersion,
                    conflictResolver));

            var task2 = Task.Run(()=>
                    eventStore.SaveEvents(
                    aggregateId,
                    newEventsSource2,
                    expectedVersion,
                    conflictResolver));

            await Task.WhenAll(task1, task2).ConfigureAwait(false);

            var actual = databaseMock.Get(aggregateId.ToString());

            Assert.NotNull(actual);
            Assert.Equal(newEventsSource1.Count + newEventsSource2.Count + initialCount, actual.Count());
            publisherMock.AssertPublishCount<Event>(newEventsSource1.Count + newEventsSource2.Count);
        }

        [Fact]
        public void FetchingAggregateRootWhenDoesntExistInDatabaseExpectException()
        {
            var aggregateId = Guid.NewGuid();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            Assert.Throws<AggregateNotFoundException>(() => eventStore.GetEventsForAggregate(aggregateId));
        }

        [Fact]
        public void FetchingAggregateRootWhenExistsGetsProperListOfEvents()
        {
            var aggregateId = Guid.NewGuid();
            var databaseMock = new DatabaseMock<EventDescriptors>();
            var expected = new EventDescriptors { Id = aggregateId };
            expected.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 0));
            expected.Add(new EventDescriptor(Guid.NewGuid(), new Event(), 1));
            databaseMock.Put(expected);
            var publisherMock = new EventPublisherMock();

            var eventStore = new EventStore(publisherMock, databaseMock);

            var actual = eventStore.GetEventsForAggregate(aggregateId);

            Assert.NotNull(actual);
            Assert.Equal(expected.Count(), actual.Count());
            Assert.Null(publisherMock.GetLastPublishedObject<Event>());
        }
    }
}
