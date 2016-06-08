using System;
using SimpleCQRS.Domain;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IRepository<TAggregate> 
        where TAggregate : AggregateRoot, new()
    {
        void Save(
            TAggregate aggregate, 
            int expectedVersion);

        TAggregate GetById(Guid aggregateId);
    }
}
