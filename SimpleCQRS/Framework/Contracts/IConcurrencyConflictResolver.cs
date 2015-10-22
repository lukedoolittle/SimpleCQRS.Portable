using System;
using System.Collections.Generic;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IConcurrencyConflictResolver
    {
        bool ConflictsWith(Type eventToCheck, IEnumerable<Type> previousEvents);
        void RegisterConflictList(Type eventDefinition, List<Type> conflictsWith);
    }
}
