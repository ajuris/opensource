using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using EPiServer.Common.Attributes;

namespace Geta.Community.EntityAttributeBuilder
{
    public static class IAttributeExtendableEntityExtensions
    {
        public static T AsAttributeExtendable<T>(this IAttributeExtendableEntity entity) where T : class, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            var interceptor = new DefaultInterceptor<T>(entity);
            var gen = new ProxyGenerator();
            var options = new ProxyGenerationOptions(new ProxyGenerationHook());

            var proxy = gen.CreateClassProxy<T>(options, interceptor);
            return proxy;
        }

        public static R GetAttributeValue<T, R>(this IAttributeExtendableEntity entity, Expression<Func<T, R>> expression)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            var helper = new ExpressionHelper();
            var attributeName = helper.ExtractMemberAccess(expression);
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
            if (typeof (R).IsGenericList())
            {
                var methodInfo = entity.GetType().GetMethods()
                    .Where(m => m.Name == "SetAttributeValue")
                    .Select(r => new { M = r, P = r.GetParameters() })
                    .Where(r => r.P[1].ParameterType.IsGenericType
                                && r.P[1].ParameterType.GetGenericTypeDefinition() == typeof (IList<>))
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
    }
}
