using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Common.Attributes;
using Attribute = EPiServer.Common.Attributes.Attribute;

namespace Geta.Community.EntityAttributeBuilder
{
    internal class EntityAttributeSynchronizer
    {
        public void SyncTypes(IEnumerable<Type> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            var defintions = BuildDefinitionList(list);
            var validator = new Validator();
            validator.ValidateDefintions(defintions);

            foreach (var definition in defintions)
            {
                foreach (var attributeMetadata in definition.MetadataAttributes)
                {
                    CreateAttribute(attributeMetadata);
                }
            }
        }

        private void AddAttribute(Type baseType, Type attributeType, string attributeName, object[] choices, bool isHidden)
        {
            IAttribute attribute = new Attribute(attributeName, baseType, attributeType);
            if (choices != null)
            {
                for (var i = 0; i < choices.Length; i = i + 2)
                {
                    attribute.Choices.Add(new AttributeValueChoice(attribute, choices[i].ToString(), choices[i + 1]));
                }
            }

            attribute.IsHidden = isHidden;
            AttributeHandler.Instance.AddAttribute(attribute);
        }

        private List<CommunityEntityDefinition> BuildDefinitionList(IEnumerable<Type> list)
        {
            var defintions = list.Select(
                type =>
                new CommunityEntityDefinition
                {
                    Type = type,
                    EntityAttribute = TypeAttributeHelper.GetAttribute(type, typeof(CommunityEntity)) as CommunityEntity
                }).ToList();

            foreach (var definition in defintions)
            {
                var properties = definition.Type.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    var hasAttribute = TypeAttributeHelper.PropertyHasAttribute(propertyInfo, typeof(CommunityEntityMetadata));
                    if (!hasAttribute)
                    {
                        continue;
                    }

                    var entityAttribute =
                        (CommunityEntityMetadata)TypeAttributeHelper.GetAttribute(propertyInfo, typeof(CommunityEntityMetadata));
                    definition.MetadataAttributes.Add(new CommunityEntityMetadataDefinition(definition, entityAttribute, propertyInfo));
                }
            }

            return defintions;
        }

        private void CreateAttribute(CommunityEntityMetadataDefinition metadata)
        {
            var attributes = AttributeHandler.Instance.GetAttributes(metadata.TargetType);
            if (attributes != null)
            {
                var attributeExist = attributes.Any(attribute => attribute != null && attribute.Name == metadata.AttributeName);
                if (!attributeExist)
                {
                    AddAttribute(metadata.TargetType, metadata.AttributeType, metadata.AttributeName, metadata.Choices, metadata.IsHidden);
                }
            }
            else
            {
                AddAttribute(metadata.TargetType, metadata.AttributeType, metadata.AttributeName, metadata.Choices, metadata.IsHidden);
            }
        }
    }
}
