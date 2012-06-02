using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.Framework.Configuration;

namespace Geta.Community.EntityAttributeBuilder
{
    internal class TypeAttributeHelper
    {
        public static Attribute GetAttribute(PropertyInfo property, Type attributeType)
        {
            return FilterAttribute(property.GetCustomAttributes(true), attributeType);
        }

        public static Attribute GetAttribute(Type type, Type attributeType)
        {
            return FilterAttribute(type.GetCustomAttributes(true), attributeType);
        }

        public static bool PropertyHasAttribute(PropertyInfo property, Type attributeType)
        {
            bool flag = false;
            foreach (object attr in property.GetCustomAttributes(true))
            {
                if (attributeType.IsInstanceOfType(attr))
                {
                    flag = true;
                }
            }

            return flag;
        }

        public static bool TypeHasAttribute(Type type, Type attributeType)
        {
            bool flag = false;
            foreach (object attr in type.GetCustomAttributes(true))
            {
                if (attributeType.IsInstanceOfType(attr))
                {
                    flag = true;
                }
            }

            return flag;
        }

        public IEnumerable<Type> GetTypesWithAttribute(Type type)
        {
            var allTypes = new List<Type>();
            foreach (Assembly assembly in GetAssemblies())
            {
                allTypes.AddRange(GetTypesWithAttributeInAssembly(type, assembly));
            }

            return allTypes;
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var config = EntityAttributeBuilderConfiguration.GetConfiguration();
            if (config.ScanAssembly != null && config.ScanAssembly.Count > 0)
            {
                return
                    AppDomain.CurrentDomain.GetAssemblies().Where(
                        a => config.ScanAssembly.Cast<AssemblyElement>().Any(
                            ae => ae.Assembly.Equals(a.GetName().Name, StringComparison.InvariantCultureIgnoreCase)));
            }

            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private static Attribute FilterAttribute(object[] attributes, Type targetType)
        {
            foreach (object attr in attributes)
            {
                if (targetType.IsInstanceOfType(attr))
                {
                    return (Attribute) attr;
                }
            }

            return null;
        }

        private IEnumerable<Type> GetTypesWithAttributeInAssembly(Type type, Assembly assembly)
        {
            try
            {
                return assembly.GetTypes().Where(t => TypeHasAttribute(t, type) && !t.IsAbstract);
            }
            catch (Exception)
            {
                // there could be situations when type could not be loaded
                // this may happen if we are traveling around *all* loaded assemblies in app domain
                return new List<Type>();
            }
        }
    }
}
