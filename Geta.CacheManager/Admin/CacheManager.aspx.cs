using System;
using EPiServer.Security;
using EPiServer.UI;

namespace Geta.CacheManager.Admin
{
    public partial class CacheManager : SystemPageBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!PrincipalInfo.HasAdminAccess)
            {
                AccessDenied();
            }
        }
    }
}