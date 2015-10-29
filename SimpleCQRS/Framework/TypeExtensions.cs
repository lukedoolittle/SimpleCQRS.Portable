using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleCQRS.Framework
{
    public static class TypeExtensions
    {
        public static string GetGenericName(this Type instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            if (!instance.GetTypeInfo().IsGenericType)
            {
                return instance.Name;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(instance.Name.Substring(0, instance.Name.LastIndexOf("`", StringComparison.Ordinal)));
            stringBuilder.Append(instance.GetTypeInfo().GenericTypeParameters.Aggregate("<",
                (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetGenericName(type)));
            stringBuilder.Append(">");

            return stringBuilder.ToString();
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
                if (argumentCount == 0)
                {
                    argumentCount = instance.GetTypeInfo().GenericTypeArguments.Count();
                }
                return instance.Name.Replace($"`{argumentCount}", "");
            }
        }
    }
}
