using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework;
using SimpleCQRS.Test.Eventing;
using SimpleCQRS.Test.Eventing.EventConstraintBaseTwo;
using SimpleCQRS.Test.Eventing.EventConstraintOne;
using SimpleCQRS.Test.Mocks;
using Xunit;

namespace SimpleCQRS.Test
{
    public class ReflectionTests
    {
        [Fact]
        public void CreatingEventHandlerAndHandlingEventResultsInActionBeingTaken()
        {
            var result = false;
            var action = new Action<object>((o) => result = true);

            var handlerOpenType = typeof (EventHandlerMock<>);
            var handlerFactoryMock = new HandlerFactoryMock(action);
            var message = new EventMock<GenericDerived1>();

            Reflection.CreateEventHandlerAndHandleEvent(
                handlerOpenType, 
                message, 
                handlerFactoryMock);

            Assert.True(result);
        }

        [Fact]
        public void GetAllConcreteImplementorsReturnsOnlyConcreteItems()
        {
            var type = typeof (BaseClass);
            var expected = new List<Type> {typeof (ConcreteDerivedClass), typeof (ConcreteDoubleDerivedClass)};

            var actual = Reflection.GetAllConcreteImplementors(type, GetType().Assembly);

            Assert.Equal(expected.Count, actual.Count());

            foreach (var actualType in actual)
            {
                Assert.True(expected.Contains(actualType));
            }
        }

        [Fact]
        public void CreateGenericTypeMakesTheCorrectType()
        {
            var genericType = typeof (DummyGenericClass<>);
            var type = typeof (DummyClass);

            var generic = Reflection.CreateGenericType(genericType, type);

            Assert.NotNull(generic);
        }

        [Fact]
        public void CreateGenericTypeWithDoubleGenericMakesAValidType()
        {
            var genericType = typeof(Event3<,>);
            var type = typeof(EventConstraintBase2);
            var subType = typeof (EventConstraintBase1);

            var generic = Reflection.CreateGenericType(genericType, new Type[] {subType, type});

            Assert.NotNull(generic);
        }

        [Fact]
        public void InvokeGenericMethodWillInvokeAPrivateGenericMethodOnAnInstance()
        {
            var instance = new DummyGenericClass<DummyClass>();
            
            Reflection.InvokeGenericMethod(instance, "GenericMethod", typeof(DummyClass));

            Assert.Equal(1, instance.CallCount);
            Assert.Equal(typeof(DummyClass).FullName, instance.LastCallType.FullName);
        }
    }

    public class DummyClass
    {
        
    }

    public class DummyGenericClass<T>
    {
        public int CallCount { get; set; }

        public Type LastCallType { get; set; }

        private void GenericMethod<K>()
        {
            CallCount++;
            LastCallType = typeof (K);
        }
    }

    public abstract class BaseClass { }
    public abstract class DerivedClass : BaseClass { }
    public class ConcreteDerivedClass : BaseClass { }
    public class ConcreteDoubleDerivedClass : DerivedClass { }
}
