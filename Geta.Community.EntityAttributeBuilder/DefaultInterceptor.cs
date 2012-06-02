using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using EPiServer.Common.Attributes;

namespace Geta.Community.EntityAttributeBuilder
{
    public class DefaultInterceptor<T> : IInterceptor
    {
        public DefaultInterceptor(IAttributeExtendableEntity entity)
        {
            Entity = entity;
        }

        public IAttributeExtendableEntity Entity { get; private set; }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("get_", StringComparison.InvariantCultureIgnoreCase))
            {
                if (IsAttributeCollection(invocation.Method.ReturnType))
                {
                    ReturnCollectionValue(invocation);
                }
                else
                {
                    ReturnSingleValue(invocation);
                }

                return;
            }

            if (invocation.Method.Name.StartsWith("set_", StringComparison.InvariantCultureIgnoreCase))
            {
                var type = invocation.Arguments[0].GetType();
                if (IsAttributeCollection(type))
                {
                    var methodInfo = Entity.GetType().GetMethods()
                        .Where(m => m.Name == "SetAttributeValue")
                        .Select(r => new { M = r, P = r.GetParameters() })
                        .Where(r => r.P[1].ParameterType.IsGenericType
                                    && r.P[1].ParameterType.GetGenericTypeDefinition() == typeof (IList<>))
                        .Select(x => x.M).SingleOrDefault();

                    if (methodInfo == null)
                    {
                        throw new InvalidOperationException("Something wrong with getting 'SetAttributeValue' method.");
                    }

                    var genericMethod = methodInfo.MakeGenericMethod(invocation.Arguments[0].GetType().GetGenericArguments().First());
                    genericMethod.Invoke(Entity, new[] { invocation.Method.Name.Substring(4), invocation.Arguments[0] });
                }
                else
                {
                    Entity.SetAttributeValue(invocation.Method.Name.Substring(4), invocation.Arguments[0]);
                }

                return;
            }

            invocation.Proceed();
        }

        private void ReturnSingleValue(IInvocation invocation)
        {
            var methodInfo = Entity.GetType().GetMethod("GetAttributeValue");
            var genericMethod = methodInfo.MakeGenericMethod(invocation.Method.ReturnType);
            invocation.ReturnValue = genericMethod.Invoke(Entity, new object[] { invocation.Method.Name.Substring(4) });
        }

        private void ReturnCollectionValue(IInvocation invocation)
        {
            var methodInfo = Entity.GetType().GetMethod("GetAttributeValues");
            var collectionItemType = invocation.Method.ReturnType.GetGenericArguments().First();
            var genericMethod = methodInfo.MakeGenericMethod(collectionItemType);
            var propertyName = invocation.Method.Name.Substring(4);

            var result = genericMethod.Invoke(Entity, new object[] { propertyName }) as IList;

            if (result != null)
            {
                var list = Activator.CreateInstance(typeof (CustomInterceptedList<>).MakeGenericType(collectionItemType),
                                                    Entity,
                                                    propertyName,
                                                    result);
                invocation.ReturnValue = list;
            }
            else
            {
                invocation.ReturnValue = null;
            }
        }

        private bool IsAttributeCollection(Type type)
        {
            var collectionType = typeof (IList<>);

            return type.IsGenericType
                   && collectionType.IsAssignableFrom(type.GetGenericTypeDefinition())
                   || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == collectionType);
        }
    }
}
