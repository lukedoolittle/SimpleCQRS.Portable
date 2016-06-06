using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Framework;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Domain
{
    public abstract class AggregateRoot
    {
        protected readonly Dictionary<Type, Action<Event>> _registeredEvents;
        protected readonly List<Event> _changes;
        protected readonly IConcurrencyConflictResolver _conflictResolver;

        public abstract Guid Id { get; }
        public int Version { get; protected set; }
        public IConcurrencyConflictResolver ConflictResolver => _conflictResolver;
        public int EventVersion { get; private set; }

        protected AggregateRoot()
        {
            _conflictResolver = new ConcurrencyConflictResolver();
            _changes = new List<Event>();
            _registeredEvents = new Dictionary<Type, Action<Event>>();
        }

        #region ConflictResolvers

        protected abstract void RegisterConflictResolvers();

        protected void RegisterConflictResolvers(
            Type targetType,
            List<Type> conflictingTypes,
            bool registerSelf)
        {
            _conflictResolver.RegisterConflictList(targetType, conflictingTypes);
            if (registerSelf)
            {
                _conflictResolver.RegisterConflictList(targetType, new List<Type> { targetType });
            }
        }

        #endregion ConflictResolvers

        #region EventRegistration

        protected abstract void RegisterEvents();

        protected void RegisterEvent<TEvent>(Action<TEvent> eventHandler)
            where TEvent : Event
        {
            _registeredEvents.Add(
                typeof(TEvent),
                @event => eventHandler(@event as TEvent));
        }

        protected void RegisterOpenGenericEvent(Type openGenericEventType)
        {
            string methodName = $"On{openGenericEventType.GetNonGenericName()}";

            _registeredEvents.Add(
                openGenericEventType,
                @event =>
                {
                    var genericTypeParameter =
                        @event.GetType().GenericTypeArguments[0];
                    new MethodInvoker(this, methodName)
                        .AddGenericParameter(genericTypeParameter)
                        .AddMethodParameter(@event)
                        .Execute();
                });
        }

        #endregion EventRegistration

        public IEnumerable<Event> GetUncommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        public void LoadsFromHistory(IEnumerable<Event> domainEvents)
        {
            var enumerableDomainEvents = domainEvents as Event[] ?? domainEvents.ToArray();

            if (!enumerableDomainEvents.Any())
                return;

            foreach (var domainEvent in enumerableDomainEvents)
            {
                Apply(domainEvent.GetType(), domainEvent);
            }

            Version = enumerableDomainEvents.Last().Version;
            EventVersion = Version;
        }

        protected void ApplyChange(Event domainEvent)
        {
            domainEvent.AggregateId = Id;
            domainEvent.Version = ++EventVersion;
            Apply(domainEvent.GetType(), domainEvent);
            _changes.Add(domainEvent);
        }

        protected void Apply(Type eventType, Event domainEvent)
        {
            Action<Event> handler;

            if (_registeredEvents.TryGetValue(eventType, out handler))
            {
                handler(domainEvent);
            }

            var unboundType = eventType.Unbind();

            if (unboundType != eventType)
            {
                if (_registeredEvents.TryGetValue(unboundType, out handler))
                {
                    handler(domainEvent);
                }
            }
        }
    }
}
