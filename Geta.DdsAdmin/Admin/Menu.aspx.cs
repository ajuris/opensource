using System;
using EPiServer.Security;
using EPiServer.UI;
using Geta.DdsAdmin.Dds;

namespace Geta.DdsAdmin.Admin
{
    public partial class Menu : SystemPageBase
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MasterPageFile = ResolveUrlFromUI("MasterPages/EPiServerUI.master");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!PrincipalInfo.HasAdminAccess)
            {
                AccessDenied();
            }

            if (IsPostBack) return;

            var explorer = new Store();
            var stores = explorer.Explore();

            repStoreTypes.DataSource = stores;
            repStoreTypes.DataBind();
        }
    }
}
