using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleCQRS.Framework
{
    public static class TypeExtensions
    {
        //TODO: remove this when we get rid of the conflict resolver
        public static Type Unbind(this Type instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            return instance.GetTypeInfo().IsGenericType ?
                    instance.GetGenericTypeDefinition() :
                    instance;
        }

        public static void InvokeMethodMatchingParameters(
            this object instance,
            params object[] parameters)
        {
            var methodInfos = instance
                .GetMethodInfosMatchingParameterSignature(
                    parameters);

            foreach (var methodInfo in methodInfos)
            {
                var method = methodInfo
                    .MakeGenericMethodFromArguments(
                        parameters);
                method.Invoke(instance, parameters);
            }
        }

        public static MethodInfo MakeGenericMethodFromArguments(
            this MethodInfo methodInfo,
            params object[] arguments)
        {
            if (!methodInfo.IsGenericMethodDefinition)
                return methodInfo;

            var methodGenericArguments = methodInfo.GetGenericArguments();
            var methodParameterTypes = methodInfo
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            var argumentTypes = arguments
                .Select(a => a.GetType())
                .ToArray();

            var engine = new TypeMapper();

            for (var i = 0; i < argumentTypes.Length; i++)
            {
                engine.MapTypes(argumentTypes[i], methodParameterTypes[i]);
            }

            var typeLookup = engine.ParameterToArgumentTypeMapping;

            return methodInfo
                .MakeGenericMethod(
                    methodGenericArguments
                        .Select(genericArgument => typeLookup[genericArgument])
                        .ToArray());
        }

        public static IEnumerable<MethodInfo> GetMethodInfosMatchingParameterSignature(
            this object instance,
            params object[] arguments)
        {
            var potentialMethods = instance
                .GetType()
                .GetTypeInfo()
                .DeclaredMethods;

            var matches = new List<MethodInfo>();
            foreach (var potentialMethod in potentialMethods)
            {
                var methodParameters = potentialMethod.GetParameters();

                if (arguments?[0] == null)
                {
                    if (methodParameters == null || methodParameters.Length == 0)
                    {
                        matches.Add(potentialMethod);
                    }
                }
                else if (methodParameters != null &&
                         methodParameters.Length == arguments.Length)
                {
                    var doAllArgumentsSatisfyParameters = !methodParameters
                        .Where((t, i) => !CanGivenTypeSatisfyParameter(
                            t.ParameterType,
                            arguments[i].GetType()))
                        .Any();

                    if (doAllArgumentsSatisfyParameters)
                    {
                        matches.Add(potentialMethod);
                    }
                }
            }

            return matches;
        }

        private static bool CanGivenTypeSatisfyParameter(
            Type parameterType,
            Type argumentType)
        {
            if (argumentType == null)
            {
                return false;
            }

            if (parameterType == argumentType)
            {
                return true;
            }

            if (argumentType.IsConstructedGenericType)
            {
                if (parameterType.GetTypeInfo().IsGenericType &&
                    parameterType.GetGenericTypeDefinition() ==
                    argumentType.GetGenericTypeDefinition())
                {
                    return true;
                }
            }
            else if (parameterType.IsGenericParameter)
            {
                if (parameterType.GetTypeInfo().IsAssignableFrom(
                    argumentType.GetTypeInfo()))
                {
                    return true;
                }
            }
            else
            {
                if (parameterType.GetTypeInfo().IsAssignableFrom(
                    argumentType.GetTypeInfo()))
                {
                    return true;
                }
            }

            return CanGivenTypeSatisfyParameter(
                parameterType,
                argumentType.GetTypeInfo().BaseType);
        }
    }
}
