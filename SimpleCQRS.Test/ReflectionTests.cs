using System;
using System.Linq;
using SimpleCQRS.Framework;
using SimpleCQRS.Test.Eventing;
using Xunit;

namespace SimpleCQRS.Test
{
    public class ReflectionTests
    {
        [Fact]
        public void GetAllTypesInNamespaceReturnsSomeNumberOfProperlyFormattedResults()
        {
            var @namespace = "Pacman.Domain.Events";

            var types = Reflection.GetAllTypesInNamespace(@namespace);

            Assert.NotNull(types);
            Assert.True(types.Any());
        }

        [Fact]
        public void GetAllTypesInNamespaceFromTypeReturnsSomeNumberOfProperlyFormattedResults()
        {
            var typeFromNamespace = typeof(EventConstraintBase1);

            var types = Reflection.GetAllTypesInNamespace(typeFromNamespace);

            Assert.NotNull(types);
            Assert.True(types.Any());
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
}
