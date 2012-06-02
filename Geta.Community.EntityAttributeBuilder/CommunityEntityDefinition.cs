using System;
using System.Collections.Generic;

namespace Geta.Community.EntityAttributeBuilder
{
    internal class CommunityEntityDefinition
    {
        private readonly List<CommunityEntityMetadataDefinition> metadataAttributes;

        public CommunityEntityDefinition()
        {
            this.metadataAttributes = new List<CommunityEntityMetadataDefinition>();
        }

        public CommunityEntity EntityAttribute { get; set; }
        public List<CommunityEntityMetadataDefinition> MetadataAttributes
        {
            get
            {
                return this.metadataAttributes;
            }
        }
        public Type Type { get; set; }
    }
}
