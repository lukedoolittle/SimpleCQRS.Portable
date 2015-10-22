using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleCQRS.Framework
{
    public static class Reflection
    {
        public static Type CreateGenericType(
            Type genericType,
            params Type[] genericParameterTypes)
        {
            var type = genericType.MakeGenericType(genericParameterTypes);
            return type;
        }

        #region Types

        public static IEnumerable<Type> GetAllTypesInNamespace(
            Type typeInNamespace,
            bool excludeGivenType = false)
        {
            var types = GetAllTypesInNamespace(typeInNamespace.Namespace);

            return excludeGivenType ? 
                types.Where(t => t.AssemblyQualifiedName != typeInNamespace.AssemblyQualifiedName) : 
                types;
        }

        public static IEnumerable<Type> GetAllTypesInNamespace(
            string @namespace, 
            Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = typeof(Reflection).GetTypeInfo().Assembly;
            }
            return assembly.DefinedTypes
                .Where(type => type.IsClass && type.Namespace == @namespace)
                .Select(t=>t.AsType());
        }

        #endregion Types

        #region Method Invocation

        public static object InvokeGenericMethod(
            object instance,
            string genericMethodName,
            Type[] genericMethodParameters,
            params object[] parameters)
        {
            MethodInfo method = instance.GetType().GetTypeInfo().GetDeclaredMethod(
                genericMethodName);
            MethodInfo generic = method.MakeGenericMethod(genericMethodParameters);
            try
            {
                return generic.Invoke(instance, parameters);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public static object InvokeGenericMethod(
            object instance, 
            string genericMethodName,
            Type genericMethodParameter,
            params object[] parameters)
        {
            return InvokeGenericMethod(instance, genericMethodName, new Type[]{ genericMethodParameter}, parameters);
        }

        #endregion MethodInvocation

        #region Subscription

        public static void SubscribeToGenericEventWithGenericHandler(
            Type eventOpenGenericType,
            Type[] eventGenericParameterTypes,
            object subscriptionObject,
            object handlerObject,
            string subscriptionMethodName,
            string handlerMethodName)
        {
            var type = Reflection.CreateGenericType(eventOpenGenericType, eventGenericParameterTypes);

            Reflection.InvokeGenericMethod(
                subscriptionObject,
                subscriptionMethodName,
                type,
                new object[] {new Action<object>(o => InvokeGenericMethod(
                                            handlerObject,
                                            handlerMethodName,
                                            eventGenericParameterTypes,
                                            new object[] {o}))});
        }

        #endregion Subscription
    }
}
