using System;
using System.Collections.Generic;

namespace SimpleCQRS.Framework
{
    public class InstanceCreator
    {
        private readonly Type _type;
        private readonly List<Type> _genericArguments;
        private readonly List<object> _constructorParameters;

        public InstanceCreator(Type type)
        {
            _type = type;

            _genericArguments = new List<Type>();
            _constructorParameters = new List<object>();
        }

        public InstanceCreator AddGenericParameter(Type generic)
        {
            _genericArguments.Add(generic);

            return this;
        }

        public InstanceCreator AddConstructorParameter(object parameter)
        {
            _constructorParameters.Add(parameter);

            return this;
        }

        public T Create<T>()
        {
            Type concreteType = _type;
            if (_genericArguments.Count > 0)
            {
                concreteType = concreteType.MakeGenericType(
                    _genericArguments.ToArray());
            }

            if (_constructorParameters.Count == 0)
            {
                return (T)Activator.CreateInstance(
                    concreteType,
                    null);
            }
            else
            {
                return (T)Activator.CreateInstance(
                    concreteType,
                    _constructorParameters.ToArray());
            }
        }
    }
}
