using System;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    public class Command : IMessage, IUnique
    {
        public Guid Id { get; private set; }
        public Guid AggregateId { get; private set; }
        public int OriginalVersion { get; private set; }
        
        public Command(
            Guid aggregateId, 
            int originalVersion)
        {
            AggregateId = aggregateId;
            OriginalVersion = originalVersion;

            Id = Guid.NewGuid();
        }
    }
}
