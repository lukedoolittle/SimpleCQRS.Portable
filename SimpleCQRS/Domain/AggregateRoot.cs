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
        protected readonly List<Event> _changes;
        protected readonly IConcurrencyConflictResolver _conflictResolver;
        protected readonly List<object> _entities;
         
        public abstract Guid Id { get; }
        public int Version { get; protected set; }
        public IConcurrencyConflictResolver ConflictResolver => _conflictResolver;
        public int EventVersion { get; private set; }

        protected AggregateRoot()
        {
            _conflictResolver = new ConcurrencyConflictResolver();
            _changes = new List<Event>();
            _entities = new List<object>();
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
                Apply(domainEvent);
            }

            Version = enumerableDomainEvents.Last().Version;
            EventVersion = Version;
        }

        protected void AddEntity(object entity)
        {
            _entities.Add(entity);
        }

        protected void RemoveEntity(object entity)
        {
            _entities.Remove(entity);
        }

        protected void ApplyChange(Event domainEvent)
        {
            domainEvent.AggregateId = Id;
            domainEvent.Version = ++EventVersion;
            Apply(domainEvent);
            _changes.Add(domainEvent);
        }

        protected void Apply(Event domainEvent)
        {
            //var eventType = domainEvent.GetType();

            //string methodName = $"On{eventType.GetNonGenericName()}";

            this.InvokeMethodMatchingParameters(domainEvent);

            //var methodInfo = this
            //    .GetMethodInfoMatchingSignature(methodName, domainEvent)
            //    .FirstOrDefault();

            //foreach (var entity in _entities)
            //{
            //    if (methodInfo != null)
            //    {
            //        break;
            //    }
            //    methodInfo = entity.
            //        GetMethodInfoMatchingSignature(methodName, domainEvent)
            //        .FirstOrDefault();
            //}

            foreach (var entity in _entities)
            {
                entity.InvokeMethodMatchingParameters(domainEvent);
            }

            //I'm not sure if this will work here...
            //if (eventType.IsConstructedGenericType)
            //{
            //    var genericTypeParameters = eventType.GenericTypeArguments;
            //    methodInfo = methodInfo.MakeGenericMethod(genericTypeParameters);
            //}

            //methodInfo.Invoke(this, new object[] { domainEvent });
        }
    }
}
