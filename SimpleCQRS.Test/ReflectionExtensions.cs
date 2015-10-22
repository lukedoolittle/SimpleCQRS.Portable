using System;
using System.Reflection;

namespace SimpleCQRS.Test
{
    public static class ReflectionExtensions
    {
        public static T GetMemberValue<T>(this object instance, string memberName)
        {
            try
            {
                var bindingFlags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static;
                
                var member = instance.GetType().GetField(memberName, bindingFlags);

                if (member != null)
                {
                    return (T) member.GetValue(instance);
                }
            }
            catch
            {
                
            }

            return default(T);
        }

        public static T GetPropertyValue<T>(this object instance, string memberName)
        {
            try
            {
                var bindingFlags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static;
                var member = instance.GetType().GetProperty(memberName, bindingFlags);

                if (member != null)
                {
                    return (T)member.GetValue(instance);
                }
            }
            catch
            {

            }

            return default(T);
        }

        public static object InvokeGenericExtensionMethod(
            Type extensionType, 
            string genericMethodName,
            Type genericMethodParameters,
            params object[] parameters)
        {
            MethodInfo method = extensionType.GetMethod(
                genericMethodName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            MethodInfo generic = method.MakeGenericMethod(genericMethodParameters);
            return generic.Invoke(null, parameters);
        }
    }
}
