using System.Linq;
using EPiServer.Framework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geta.Community.EntityAttributeBuilder.Tests
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void TestConfiguration_WithSingleAssemblyAdded()
        {
            var config = EntityAttributeBuilderConfiguration.GetConfiguration();
            var list = config.ScanAssembly.Cast<AssemblyElement>();
            Assert.AreEqual(1, list.Count());
        }
    }
}
