using System;
using System.Collections.Generic;
using SimpleCQRS.Domain;
using SimpleCQRS.Test.Eventing;
using SimpleCQRS.Test.Eventing.EventConstraintOne;
using Xunit;

namespace SimpleCQRS.Test
{
    public class ConcurrencyConflictResolverTests
    {
        [Fact]
        public void RegisterConflictingOpenTypesTryToConflictWithEventWithSameGenericParameter()
        {
            var targetType = typeof(Event2<>);
            var conflictingTypes = new List<Type>
            {
                typeof(Event2<>)
            };
            var actualType = typeof (Event2<EventConstraint1>);
            var actualConflictingTypes = new List<Type> {typeof (Event2<EventConstraint1>)};
            var resolver = new ConcurrencyConflictResolver();

            resolver.RegisterConflictList(targetType, conflictingTypes);

            Assert.True(resolver.ConflictsWith(actualType, actualConflictingTypes));
        }

        [Fact]
        public void RegisterConflictingOpenTypesTryToConflictWithEventWithDifferentGenericParameter()
        {
            var targetType = typeof(Event2<>);
            var conflictingTypes = new List<Type>
            {
                typeof(Event2<>)
            };
            var actualType = typeof(Event2<EventConstraint1>);
            var actualConflictingTypes = new List<Type> { typeof(Event2<EventConstraintAnother1>) };
            var resolver = new ConcurrencyConflictResolver();

            resolver.RegisterConflictList(targetType, conflictingTypes);

            Assert.False(resolver.ConflictsWith(actualType, actualConflictingTypes));
        }

        [Fact]
        public void RegisterConflictingTypesAndExpectHasConflictToReturnTrue()
        {
            var targetType = typeof(Event1);
            var conflictingTypes = new List<Type>
            {
                typeof(Event1),
                typeof(Event2<EventConstraint1>)
            };
            var resolver = new ConcurrencyConflictResolver();

            resolver.RegisterConflictList(targetType, conflictingTypes);

            foreach (var conflictingType in conflictingTypes)
            {
                Assert.True(resolver.ConflictsWith(targetType, new List<Type> {conflictingType}));
            }
        }

        [Fact]
        public void RegisterEmptyConflictingTypeListExpectNoConflicts()
        {
            var targetType = typeof(Event1);
            var conflictingTypes = new List<Type>
            {
                typeof(Event1),
                typeof(Event2<EventConstraint1>)
            };
            var resolver = new ConcurrencyConflictResolver();

            resolver.RegisterConflictList(targetType, new List<Type>());

            foreach (var conflictingType in conflictingTypes)
            {
                Assert.False(resolver.ConflictsWith(targetType, new List<Type> { conflictingType }));
            }
        }

        [Fact]
        public void RegisterNonEventTypeExpectException()
        {
            var targetType = typeof(EventConstraintBase1);
            var conflictingTypes = new List<Type>
            {
                typeof(Event1),
                typeof(Event2<EventConstraint1>)
            };
            var resolver = new ConcurrencyConflictResolver();

            Assert.Throws<ArgumentException>(() => 
                resolver.RegisterConflictList(targetType, conflictingTypes));
        }

        [Fact]
        public void RegisterNonEventConflictListExpectException()
        {
            var targetType = typeof (Event1);
            var conflictingTypes = new List<Type>
            {
                typeof(EventConstraintBase1),
                typeof(Event2<EventConstraint1>)
            };
            var resolver = new ConcurrencyConflictResolver();

            Assert.Throws<ArgumentException>(() =>
                resolver.RegisterConflictList(targetType, conflictingTypes));
        }

        [Fact]
        public void RegisterAndThenRegisterAgainAddsThoseToConflictList()
        {
            var targetType = typeof(Event1);
            var conflictingTypesOne = new List<Type>
            {
                typeof(Event1),
                typeof(Event2<EventConstraint1>)
            };
            var conflictingTypesTwo = new List<Type>
            {
                typeof (Event2<EventConstraint1>)
            };
            var resolver = new ConcurrencyConflictResolver();

            resolver.RegisterConflictList(targetType, conflictingTypesOne);
            resolver.RegisterConflictList(targetType, conflictingTypesTwo);

            foreach (var conflictingType in conflictingTypesTwo)
            {
                Assert.True(resolver.ConflictsWith(targetType, new List<Type> { conflictingType }));
            }

            foreach (var conflictingType in conflictingTypesOne)
            {
                Assert.True(resolver.ConflictsWith(targetType, new List<Type> { conflictingType }));
            }
        }
    }
}
