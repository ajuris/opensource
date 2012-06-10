using System;
using System.Collections.Generic;
using System.Linq;

namespace Geta.Community.EntityAttributeBuilder
{
    public static class ReflectionHelper
    {
        public static bool IsGenericList(this Type type)
        {
            var collectionType = typeof (IList<>);

            return type.IsGenericType
                   && collectionType.IsAssignableFrom(type.GetGenericTypeDefinition())
                   || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == collectionType);
        }
    }
}
