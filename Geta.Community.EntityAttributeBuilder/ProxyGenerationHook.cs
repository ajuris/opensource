using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Geta.Community.EntityAttributeBuilder
{
    public class ProxyGenerationHook : IProxyGenerationHook
    {
        public void MethodsInspected()
        {
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
            if (TypeAttributeHelper.PropertyHasAttribute(memberInfo.GetProperty(), typeof(CommunityEntityMetadata)))
            {
                throw new InvalidOperationException(string.Format("Property named '{0}' for type '{1}' is not marked as virtual.",
                                                                  memberInfo.GetProperty().Name,
                                                                  type));
            }
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            // we are interested only in properties for now
            return methodInfo.Name.StartsWith("get_", StringComparison.Ordinal)
                   || methodInfo.Name.StartsWith("set_", StringComparison.Ordinal);
        }
    }
}
