using System;

namespace SimpleCQRS.Exceptions
{
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }

        public Guid AggregateId { get; }
    }
}
