using System.Configuration;
using EPiServer.Framework.Configuration;

namespace Geta.Community.EntityAttributeBuilder
{
    public class EntityAttributeBuilderConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("scanAssembly", IsRequired = false)]
        public AssemblyElementCollection ScanAssembly
        {
            get { return (AssemblyElementCollection) this["scanAssembly"]; }
        }

        [ConfigurationProperty("xmlns", IsRequired = false)]
        public string XmlNamespace
        {
            get { return "http://EntityAttributeBuilder.Configuration.EntityAttributeBuilderConfiguration"; }
        }

        public static EntityAttributeBuilderConfiguration GetConfiguration()
        {
            var configuration = ConfigurationManager.GetSection("entityAttributeBuilder") as EntityAttributeBuilderConfiguration;
            return configuration ?? new EntityAttributeBuilderConfiguration();
        }
    }
}
