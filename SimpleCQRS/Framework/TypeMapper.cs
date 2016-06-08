using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleCQRS.Framework
{
    public class TypeMapper
    {
        public Dictionary<Type, Type> ParameterToArgumentTypeMapping { get; } =
            new Dictionary<Type, Type>();

        public void MapTypes(
            Type argumentType,
            Type parameterType)
        {
            if (argumentType == null)
            {
                return;
            }

            if (parameterType.IsGenericParameter)
            {
                if (ParameterToArgumentTypeMapping.ContainsKey(parameterType))
                {
                    if (ParameterToArgumentTypeMapping[parameterType] !=
                        argumentType)
                        throw new ArgumentException();
                }
                else
                {
                    ParameterToArgumentTypeMapping.Add(
                        parameterType,
                        argumentType);
                }
            }
            else if (parameterType.GetTypeInfo().IsGenericType)
            {
                if (!argumentType.GetTypeInfo().IsGenericType)
                {
                    MapTypes(argumentType.GetTypeInfo().BaseType, parameterType);
                }
                else
                {
                    var concreteTypeArgs = argumentType
                        .GetTypeInfo()
                        .GenericTypeArguments;
                    var genericTypeArgs = parameterType
                        .GetTypeInfo()
                        .GenericTypeArguments;
                    for (var i = 0; i < genericTypeArgs.Length; i++)
                    {
                        MapTypes(concreteTypeArgs[i], genericTypeArgs[i]);
                    }
                }
            }
        }
    }
}
