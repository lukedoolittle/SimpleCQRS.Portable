using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pacman.Framework
{
    public class MethodInvoker
    {
        private readonly object _instance;
        private readonly string _methodName;
        private readonly List<Type> _genericArguments;
        private readonly List<object> _methodParameters;


        public MethodInvoker(object instance, string methodName)
        {
            _instance = instance;
            _methodName = methodName;

            _genericArguments = new List<Type>();
            _methodParameters = new List<object>();
        }

        public MethodInvoker AddGenericParameter(Type generic)
        {
            _genericArguments.Add(generic);

            return this;
        }

        public MethodInvoker AddMethodParameter(object parameter)
        {
            _methodParameters.Add(parameter);

            return this;
        }

        public T Execute<T>()
        {
            return (T) Execute();
        }

        public object Execute()
        {
            var methodInfo = GetMethodInfo();

            if (_genericArguments.Count > 0)
            {
                methodInfo = methodInfo.MakeGenericMethod(_genericArguments.ToArray());
            }

            try
            {
                if (_methodParameters.Count > 0)
                {
                    return methodInfo.Invoke(_instance, _methodParameters.ToArray());
                }
                else
                {
                    return methodInfo.Invoke(_instance, null);
                }
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        private MethodInfo GetMethodInfo()
        {
            //NOTE: this may not return the correct method in some overload situations
            return _instance
                .GetType()
                .GetRuntimeMethods()
                .Single(m =>
                {
                    if (m.Name != _methodName)
                        return false;

                    var methodParameters = m.GetParameters();
                    if ((methodParameters == null || methodParameters.Length == 0))
                        return _methodParameters.Count == 0;
                    return _methodParameters.Count == methodParameters.Length;
                });
        }
    }
}
