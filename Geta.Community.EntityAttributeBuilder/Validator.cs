using System;
using System.Collections.Generic;
using System.Linq;

namespace Geta.Community.EntityAttributeBuilder
{
    internal class Validator
    {
        public void ValidateDefintions(List<CommunityEntityDefinition> defintions)
        {
            foreach (CommunityEntityDefinition definition in defintions)
            {
                if (definition.EntityAttribute.TargetType == null)
                {
                    throw new InvalidOperationException(string.Format("Metadata on '{0}' type do not specify target entity type.",
                                                                      definition.Type));
                }

                if (definition.MetadataAttributes != null && definition.MetadataAttributes.Any())
                {
                    foreach (CommunityEntityMetadataDefinition entityMetadataInfo in definition.MetadataAttributes)
                    {
                        if (entityMetadataInfo.Choices != null && entityMetadataInfo.Choices.Length % 2 != 0)
                        {
                            throw new InvalidOperationException(
                                string.Format("Choices for attribute '{0}' for type '{1}' must be even count. Actual count - {2}",
                                              entityMetadataInfo.AttributeName,
                                              definition.EntityAttribute.TargetType,
                                              definition.MetadataAttributes.Count));
                        }
                    }
                }
            }
        }
    }
}
