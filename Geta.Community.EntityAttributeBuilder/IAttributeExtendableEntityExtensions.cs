using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Castle.DynamicProxy;
using EPiServer.Common.Attributes;

namespace Geta.Community.EntityAttributeBuilder
{
    public static class IAttributeExtendableEntityExtensions
    {
        private static readonly ProxyGenerator gen = new ProxyGenerator();
        private static readonly ProxyGenerationOptions options = new ProxyGenerationOptions(new ProxyGenerationHook());

        private static readonly ConditionalWeakTable<IAttributeExtendableEntity, object> table =
            new ConditionalWeakTable<IAttributeExtendableEntity, object>();

        public static T AsAttributeExtendable<T>(this IAttributeExtendableEntity entity) where T : class, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            object classProxyCached;

            // try to find already created class proxy from the cache
            // this is required to increase speed a bit and not to create new class proxy for the same class more than once
            table.TryGetValue(entity, out classProxyCached);

            if (classProxyCached == null)
            {
                classProxyCached = GenerateClassProxy<T>(entity);
                table.Add(entity, classProxyCached);
            }

            return classProxyCached as T;
        }

        public static R GetAttributeValue<T, R>(this IAttributeExtendableEntity entity, Expression<Func<T, R>> expression)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            var helper = new ExpressionHelper();
            var attributeName = helper.ExtractMemberAccess(expression);

            if (typeof(R).IsGenericList())
            {
                // TODO: add support for IList attributes
            }

            return entity.GetAttributeValue<R>(attributeName);
        }

        public static void SetAttributeValue<TMetadata, R>(this IAttributeExtendableEntity entity,
                                                           Expression<Func<TMetadata, R>> expression,
                                                           R value)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var helper = new ExpressionHelper();
            var attributeName = helper.ExtractMemberAccess(expression);
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new InvalidOperationException("Could not extract member access expression from + " + expression);
            }

            // check if value is list, if so - we need to find precise overloaded method to invoke
            if (typeof(R).IsGenericList())
            {
                var methodInfo = entity.GetType().GetMethods()
                    .Where(m => m.Name == "SetAttributeValue")
                    .Select(r => new { M = r, P = r.GetParameters() })
                    .Where(r => r.P[1].ParameterType.IsGenericType
                                && r.P[1].ParameterType.GetGenericTypeDefinition() == typeof(IList<>))
                    .Select(x => x.M).SingleOrDefault();

                if (methodInfo == null)
                {
                    throw new InvalidOperationException("Something wrong with getting 'SetAttributeValue' method for type '" +
                                                        entity.GetType().FullName + "'.");
                }

                var genericMethod = methodInfo.MakeGenericMethod(typeof(R).GetGenericArguments().First());
                genericMethod.Invoke(entity, new object[] { attributeName, value });
            }
            else
            {
                entity.SetAttributeValue(attributeName, value);
            }
        }

        private static T GenerateClassProxy<T>(IAttributeExtendableEntity entity) where T : class, new()
        {
            var interceptor = new DefaultInterceptor<T>(entity);
            var proxy = gen.CreateClassProxy<T>(options, interceptor);

            return proxy;
        }
    }
}
