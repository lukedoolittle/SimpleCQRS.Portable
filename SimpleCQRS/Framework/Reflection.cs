﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleCQRS.Framework.Contracts;

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

        public static IEnumerable<Type> GetAllConcreteImplementors(
            Type baseType, 
            Assembly assembly)
        {
            return assembly.DefinedTypes
                .Where(type => !type.IsAbstract && type.AsType().HasBase(baseType))
                .Select(t => t.AsType());
        } 

        #endregion Types

        #region Method Invocation

        public static object InvokeGenericMethod(
            object instance,
            string genericMethodName,
            Type[] genericMethodParameters,
            params object[] parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var method = instance.GetType().GetTypeInfo().GetMethodInfo(
                genericMethodName);

            if (method == null)
            {
                throw new Exception($"No method found named {genericMethodName}");
            }

            if (!method.IsGenericMethod)
            {
                throw new Exception($"Generic method {method.Name} is not generic");
            }

            if (genericMethodParameters == null || genericMethodParameters.Any(p => p == null))
            {
                throw new Exception($"Generic method paramaters for generic method {method.Name} invalid");
            }

            var generic = method.MakeGenericMethod(genericMethodParameters);

            if (generic == null)
            {
                throw new Exception($"Could not create generic type from {method.Name}");
            }

            try
            {
                return generic.Invoke(instance, parameters);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        private static MethodInfo GetMethodInfo(
            this TypeInfo instance, 
            string methodName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            return instance.GetDeclaredMethod(methodName) ?? 
                instance.BaseType.GetTypeInfo().GetMethodInfo(methodName);
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

        public static void CreateEventHandlerAndHandleEvent(
            Type eventHandlerOpenType, 
            IMessage message,
            IHandlerFactory factory)
        {
            var genericArguments = message.GetType().GenericTypeArguments;

            Type closedHandlerType = eventHandlerOpenType.MakeGenericType(genericArguments);

            var handler = factory.CreateHandler(closedHandlerType);

            new MethodInvoker(handler, "Handle")
                .AddMethodParameter(message)
                .Execute();
        }

        #endregion Subscription
    }
}
