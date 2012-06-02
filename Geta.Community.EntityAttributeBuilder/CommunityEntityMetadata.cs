using System;

namespace Geta.Community.EntityAttributeBuilder
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CommunityEntityMetadata : Attribute
    {
        public object[] Choices { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsHidden { get; set; }
    }
}
