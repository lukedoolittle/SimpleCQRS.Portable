using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleCQRS.Framework;
using Xunit;

namespace SimpleCQRS.Test
{
    public class ReflectionTests
    {
        [Fact]
        public void FactNoArgument()
        {
            var instance = new MyClassOfMethods();
            instance.MethodB();

            Test(instance, null);
        }

        [Fact]
        public void FactNoGenericsButDerived()
        {
            var instance = new MyClassOfMethods();
            var argument = new ConstraintB();
            instance.MethodC(argument);

            Test(instance, argument);
        }

        [Fact]
        public void Fact()
        {
            var instance = new MyClassOfMethods();
            var argument = new ConstraintA();
            instance.MethodD(argument);


            Test(instance, argument);
        }

        [Fact]
        public void TestDerived()
        {
            var instance = new MyClassOfMethods();
            var argument = new ConstraintB();
            instance.MethodD(argument);

            Test(instance, argument);
        }

        [Fact]
        public void FactEmbedded()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyClass<ConstraintA>();
            instance.MethodE(argument);

            Test(instance, argument);
        }

        [Fact]
        public void FactEmbeddedAndDerived()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyClass<ConstraintB>();
            instance.MethodE(argument);

            Test(instance, argument);
        }

        [Fact]
        public void FactInherited()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyThirdClass();
            instance.MethodE(argument);

            Test(instance, argument);
        }

        [Fact]
        public void InvokeMethodAllInOne()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyThirdClass();

            instance.InvokeMethodMatchingParameters(argument);
        }

        private void Test(object instance, object argument)
        {
            IEnumerable<MethodInfo> methods = instance.GetMethodInfosMatchingParameterSignature(argument);
            Assert.True(methods.Count() == 1);

            var method = methods.First();

            if (argument != null)
            {
                var constructedMethod = method.MakeGenericMethodFromArguments(argument);

                constructedMethod.Invoke(instance, new object[] { argument });
            }
            else
            {
                method.Invoke(instance, null);
            }
        }
    }


    public class ConstraintA { }
    public class ConstraintB : ConstraintA { }
    public class MyClass<T> where T : ConstraintA { }
    public class MySecondClass<T> where T : ConstraintA { }
    public class MyThirdClass : MySecondClass<ConstraintB> { }
    public class MyClassOfMethods
    {
        public void MethodB() { }
        public void MethodC(ConstraintA someParameter) { }
        public void MethodD<T>(T someParameter) where T : ConstraintA { }
        public void MethodE<T>(MyClass<T> someParameter) where T : ConstraintA { }
        public void MethodE<T>(MySecondClass<T> someParameter) where T : ConstraintB { }
    }
}
