using System;
using System.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Infrastructure;
using SimpleCQRS.Test.Mocks;
using Xunit;

namespace SimpleCQRS.Test
{
    public class RepositoryTests
    {
        [Fact]
        public void SavingAnAggregateWithNoEvents()
        {
            var storageMock = new EventStoreMock();
            var repository = new Repository<AggregateRootMock>(storageMock);

            var rootMock = new AggregateRootMock(Guid.NewGuid());

            repository.Save(rootMock, 0);

            storageMock.AssertSaveCalls(1);
        }

        [Fact]
        public void SavingAnAggregateWithSomeEventsStoresThoseEventsInTheEventStore()
        {
            var storageMock = new EventStoreMock();
            var repository = new Repository<AggregateRootMock>(storageMock);

            var rootMock = new AggregateRootMock(Guid.NewGuid());
            var @event = new Event();
            rootMock.SomeEvent(@event);

            repository.Save(rootMock, 0);

            storageMock.AssertSaveCalls(1);
            Assert.Equal(@event, rootMock.MyEvent);
            Assert.Equal(1, storageMock.LatestEvents.Count());
            var storedEvent = storageMock.LatestEvents.First();
            Assert.Equal(@event, storedEvent);
        }

        [Fact]
        public void GetAnAggregateWithSomeEventsReturnsEvents()
        {
            var aggregateId = Guid.NewGuid();
            var @event = new Event();
            var storageMock = new EventStoreMock().SeedEvents(aggregateId, @event);
            var repository = new Repository<AggregateRootMock>(storageMock);

            var rootMock = new AggregateRootMock(aggregateId);
            rootMock.SomeEvent(@event);

            repository.Save(rootMock, 0);
            var actualRoot = repository.GetById(aggregateId);
            
            Assert.Equal(@event, actualRoot.MyEvent);
        }
    }
}
