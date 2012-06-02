using System;

namespace Geta.Community.EntityAttributeBuilder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CommunityEntity : Attribute
    {
        public Type TargetType { get; set; }
    }
}
