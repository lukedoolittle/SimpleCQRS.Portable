using System;
using SimpleCQRS.Domain;

namespace SimpleCQRS.Test.Mocks
{
    class AggregateRootMock : AggregateRoot
    {
        public override Guid Id { get; }

        public Event MyEvent { get; private set; }

        public AggregateRootMock()
        {
            RegisterEvents();
        }

        public AggregateRootMock(Guid id) : this()
        {
            Id = id;
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<Event>(OnSomeEvent);
        }

        protected override void RegisterConflictResolvers()
        {
            throw new NotImplementedException();
        }

        protected void OnSomeEvent(Event @event)
        {
            MyEvent = @event;
        }

        public void SomeEvent(Event @event)
        {
            ApplyChange(@event);
        }
    }
}
