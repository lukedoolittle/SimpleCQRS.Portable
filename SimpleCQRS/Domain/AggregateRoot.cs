using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleCQRS.Framework;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Domain
{
    public abstract class AggregateRoot
    {
        protected readonly Dictionary<Type, Action<Event>> _registeredEvents;
        protected readonly List<Event> _changes;
        protected readonly IConcurrencyConflictResolver _conflictResolver;
        protected readonly Assembly _genericArgumentsAssembly;

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

        protected AggregateRoot(Assembly genericArgumentsAssembly) : 
            this()
        {
            _genericArgumentsAssembly = genericArgumentsAssembly;
        }

        #region ConflictResolvers

        protected abstract void RegisterConflictResolvers();

        protected void RegisterGenericConflictResolvers<TGenericType, TSubGenericType>(
            Type genericEventType,
            List<Type> conflictingTypes,
            bool registerSelf = false)
        {
            var generic = typeof(TGenericType);
            var genericSub = typeof(TSubGenericType);

            var genericTypes =
                Reflection.GetAllConcreteImplementors(
                generic,
                _genericArgumentsAssembly);

            var subGenericTypes =
                Reflection.GetAllConcreteImplementors(
                genericSub,
                _genericArgumentsAssembly);

            foreach (var genericType in genericTypes)
            {
                foreach (var subGenericType in subGenericTypes)
                {
                    var targetType = Reflection.CreateGenericType(
                        genericEventType, 
                        genericType, 
                        subGenericType);
                    var genericConflictingTypes = conflictingTypes
                        .Select(t =>
                            t.GetTypeInfo().IsGenericTypeDefinition ?
                                Reflection.CreateGenericType(t, genericType, subGenericType) :
                                t);
                    RegisterConflictResolvers(targetType, genericConflictingTypes.ToList(), registerSelf);
                }
            }
        }

        protected void RegisterGenericConflictResolvers<TGenericType>(
            Type genericEventType,
            List<Type> conflictingTypes,
            bool registerSelf = false)
        {
            var generic = typeof(TGenericType);

            var genericTypes = Reflection.GetAllConcreteImplementors(
                generic,
                _genericArgumentsAssembly);

            foreach (var type in genericTypes)
            {
                var genericType = Reflection.CreateGenericType(genericEventType, type);
                var genericConflictingTypes = conflictingTypes
                    .Select(t => 
                        t.GetTypeInfo().IsGenericTypeDefinition ? 
                            Reflection.CreateGenericType(t, type) : 
                            t);
                RegisterConflictResolvers(genericType, genericConflictingTypes.ToList(), registerSelf);
            }
        }

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

        //This is a bit of a hack as it simply combinatorically registers all possible
        //type subtype combinations even if they are not valid
        protected void RegisterGenericEvents<TGenericType, TSubGenericType>(Type genericEventType)
        {
            var generic = typeof(TGenericType);
            var genericSub = typeof(TSubGenericType);

            var genericTypes =
                Reflection.GetAllConcreteImplementors(
                generic,
                _genericArgumentsAssembly);

            var subGenericTypes =
                Reflection.GetAllConcreteImplementors(
                genericSub,
                _genericArgumentsAssembly);

            string methodName = $"On{genericEventType.GetNonGenericName()}";

            foreach (var genericType in genericTypes)
            {
                foreach (var subGenericType in subGenericTypes)
                {
                    Reflection.SubscribeToGenericEventWithGenericHandler(
                        genericEventType,
                        new Type[] { genericType, subGenericType },
                        this,
                        this,
                        "RegisterEvent",
                        methodName);
                }
            }
        }

        protected void RegisterGenericEvents<TGenericType>(Type genericEventType)
        {
            string methodName = $"On{genericEventType.GetNonGenericName()}";

            var generic = typeof(TGenericType);

            var genericTypes = Reflection.GetAllConcreteImplementors(
                generic,
                _genericArgumentsAssembly);

            foreach (var genericType in genericTypes)
            {
                Reflection.SubscribeToGenericEventWithGenericHandler(
                    genericEventType,
                    new Type[] { genericType },
                    this,
                    this,
                    "RegisterEvent",
                    methodName);
            }
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
        }
    }
}
