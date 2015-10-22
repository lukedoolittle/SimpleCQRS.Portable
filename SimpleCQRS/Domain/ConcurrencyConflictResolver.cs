using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Domain
{
    public class ConcurrencyConflictResolver : IConcurrencyConflictResolver
    {
        private readonly Dictionary<Type, List<Type>> _conflictRegister;

        public ConcurrencyConflictResolver()
        {
            _conflictRegister = new Dictionary<Type, List<Type>>();
        }

        public bool ConflictsWith(Type eventToCheck, IEnumerable<Type> previousEvents)
        {
            return 
                !_conflictRegister.ContainsKey(eventToCheck) || 
                previousEvents.Any(previousEvent => _conflictRegister[eventToCheck].Any(et => et == previousEvent));
        }

        public void RegisterConflictList(Type eventDefinition, List<Type> conflictsWith)
        {
            if (!eventDefinition.GetTypeInfo().IsSubclassOf(typeof (Event)))
            {
                throw new ArgumentException();
            }
            if (conflictsWith.Any(c => c.GetTypeInfo().IsSubclassOf(typeof (Event)) == false))
            {
                throw new ArgumentException();
            }

            if (_conflictRegister.ContainsKey(eventDefinition))
            {
                var currentConflicts = _conflictRegister[eventDefinition];
                currentConflicts.AddRange(conflictsWith);
            }
            else
            {
                _conflictRegister.Add(eventDefinition, conflictsWith);
            }
        }
    }
}
