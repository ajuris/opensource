using System;
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

            entity.SetAttributeValue(attributeName, value);
        }
    }
}
