using System;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    public class Repository<T> : IRepository<T> 
        where T : AggregateRoot, new()
    {
        private readonly IEventStore _storage;

        public Repository(IEventStore storage)
        {
            _storage = storage;
        }

        public void Save(
            T aggregate, 
            int expectedVersion)
        {
            _storage.SaveEvents(
                aggregate.Id, 
                aggregate.GetUncommittedChanges(), 
                expectedVersion,
                aggregate.ConflictResolver);
        }

        public T GetById(Guid id)
        {
            var obj = new T();
            var e = _storage.GetEventsForAggregate(id);
            obj.LoadsFromHistory(e);
            return obj;
        }
    }
}
