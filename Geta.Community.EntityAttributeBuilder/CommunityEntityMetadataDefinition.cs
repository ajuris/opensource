using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Geta.Community.EntityAttributeBuilder
{
    internal class CommunityEntityMetadataDefinition
    {
        public CommunityEntityMetadataDefinition(CommunityEntityDefinition entityDefinition,
                                                 CommunityEntityMetadata metdata,
                                                 PropertyInfo propertyInfo)
        {
            if (entityDefinition == null)
            {
                throw new ArgumentNullException("entityDefinition");
            }

            if (metdata == null)
            {
                throw new ArgumentNullException("metdata");
            }
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            TargetType = entityDefinition.EntityAttribute.TargetType;
            Choices = metdata.Choices;
            IsHidden = metdata.IsHidden;

            var attributeName = propertyInfo.Name;
            if (!string.IsNullOrEmpty(metdata.Name))
            {
                attributeName = metdata.Name;
            }

            AttributeName = attributeName;

            var attrType = propertyInfo.PropertyType;
            if (metdata.Type != null)
            {
                attrType = metdata.Type;
            }

            if (attrType.IsGenericType)
            {
                // check if attribute data type is collection of something then we need to extract generic parameter type

                var definitionType = attrType.GetGenericTypeDefinition();
                if (definitionType == typeof(IList<>))
                {
                    var elementType = attrType.GetGenericArguments().FirstOrDefault();
                    if (elementType == null)
                    {
                        throw new ArgumentException("Failed to retrieve generic arguments for type '" + attrType + "'");
                    }

                    attrType = elementType;
                }
            }

            AttributeType = attrType;
        }

        public string AttributeName { get; set; }
        public Type AttributeType { get; set; }
        public object[] Choices { get; set; }
        public Type TargetType { get; set; }
        public bool IsHidden { get; set; }
        public bool IsCollection { get; set; }
    }
}
