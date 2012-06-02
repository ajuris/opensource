using System;
using EPiServer.Security;
using EPiServer.Shell.Navigation;
using EPiServer.UI;

namespace Geta.DdsAdmin.Admin
{
    [MenuSection(MenuPaths.Global + "/geta_newsletter/admin")]
    public partial class Default : SystemPageBase
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MasterPageFile = ResolveUrlFromUI("MasterPages/Frameworks/Framework.master");
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
