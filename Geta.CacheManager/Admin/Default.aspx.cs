using System;
using EPiServer.Security;
using EPiServer.Shell.Navigation;
using EPiServer.UI;

namespace Geta.CacheManager.Admin
{
    [MenuSection(MenuPaths.Global + "/geta_cachemanager/admin")]
    public partial class Default : SystemPageBase
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MasterPageFile = ResolveUrlFromUI("MasterPages/Frameworks/Framework.Master");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!PrincipalInfo.HasAdminAccess)
            {
                AccessDenied();
            }
        }
    }
}