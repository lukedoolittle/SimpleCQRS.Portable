using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleCQRS.Framework
{
    public static class TypeExtensions
    {
        public static string FullNameForProperty(this Type instance)
        {
            return $"{instance.Namespace}.{instance.Name}";
        }

        public static string AssemblyQualifiedShortName(
            this Type instance)
        {
            return $"{instance.FullName}, {instance.GetTypeInfo().Assembly.GetName().Name}";
        }

        public static string GetGenericName(this Type instance)
        {
            if (!instance.GetTypeInfo().IsGenericType)
                return instance.Name;
            StringBuilder sb = new StringBuilder();

            sb.Append(instance.Name.Substring(0, instance.Name.LastIndexOf("`")));
            sb.Append(instance.GetTypeInfo().GenericTypeParameters.Aggregate("<",
                (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetGenericName(type)
                ));
            sb.Append(">");

            return sb.ToString();
        }

        public static string GetNonGenericName(this Type instance)
        {
            if (!instance.GetTypeInfo().IsGenericType)
            {
                return instance.Name;
            }
            else
            {
                var argumentCount = instance.GetTypeInfo().GenericTypeParameters.Count();
                return instance.Name.Replace($"`{argumentCount}", "");
            }
        }
    }
}
