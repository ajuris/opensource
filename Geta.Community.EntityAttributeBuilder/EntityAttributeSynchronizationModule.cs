using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace Geta.Community.EntityAttributeBuilder
{
    [InitializableModule]
    [ModuleDependency((typeof(InitializationModule)))]
    public class EntityAttributeSynchronizationModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            context.InitComplete += ContextInitComplete;
        }

        public void Preload(string[] parameters)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
            context.InitComplete -= ContextInitComplete;
        }

        private void ContextInitComplete(object sender, EventArgs e)
        {
            var helper = new TypeAttributeHelper();
            var sync = new EntityAttributeSynchronizer();

            var list = helper.GetTypesWithAttribute(typeof(CommunityEntity));
            sync.SyncTypes(list);
        }
    }
}
