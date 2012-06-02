using System;
using System.Linq;
using System.Reflection;

namespace Geta.Community.EntityAttributeBuilder
{
    public static class MemberInfoExtensions
    {
        public static PropertyInfo GetProperty(this MemberInfo member)
        {
            if (member.MemberType != MemberTypes.Method)
            {
                throw new InvalidOperationException("Cannot get property info from '" + member.MemberType + "'");
            }

            var method = member as MethodInfo;
            if (method == null)
            {
                throw new InvalidOperationException("Failed to cast '" + member + "' to MethodInfo");
            }

            bool takesArg = method.GetParameters().Length == 1;
            bool hasReturn = method.ReturnType != typeof(void);

            if (method.DeclaringType == null)
            {
                throw new InvalidOperationException("DeclaringType of '" + member + "' is null");
            }

            if (takesArg == hasReturn)
            {
                return null;
            }

            if (takesArg)
            {
                return method.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetSetMethod() == method);
            }

            return method.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetGetMethod() == method);
        }
    }
}
