using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleCQRS.Domain;
using SimpleCQRS.Test.Eventing;
using SimpleCQRS.Test.Eventing.EventConstraintBaseTwo;
using SimpleCQRS.Test.Eventing.EventConstraintOne;

namespace SimpleCQRS.Test.Mocks
{
    public class AggregateRootRegistrationMock : AggregateRoot
    {
        private readonly List<object> _eventsHandled;
        public override Guid Id { get; }

        public AggregateRootRegistrationMock()
        {
            _eventsHandled = new List<object>();
        }

        public TEventType GetEvent<TEventType>()
        {
            return (TEventType)_eventsHandled.First(e => e is TEventType);
        }

        public void OnEvent1(Event1 @event)
        {
            _eventsHandled.Add(@event);
        }

        public void OnEvent2<TEventConstraintBase1>(
            Event2<TEventConstraintBase1> @event)
            where TEventConstraintBase1 : EventConstraintBase1
        {
            _eventsHandled.Add(@event);
        }

        public void OnEvent3<TEventConstraintBase1, TEventConstraintBase2>(
            Event3<TEventConstraintBase1, TEventConstraintBase2> @event)
            where TEventConstraintBase1 : EventConstraintBase1
            where TEventConstraintBase2 : EventConstraintBase2
        {
            _eventsHandled.Add(@event);
        }

        protected override void RegisterConflictResolvers()
        {
            throw new NotImplementedException();
        }

        public void CallRegisterEvents()
        {
            RegisterEvents();
        }

        public void RegisterOpenGenericEvents(Type eventType)
        {
            RegisterOpenGenericEvent(eventType);
        }

        public void HandleEvent<TEvent>(TEvent @event)
            where TEvent : Event
        {
            ApplyChange(@event);
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<Event1>(OnEvent1);
        }
    }
}
