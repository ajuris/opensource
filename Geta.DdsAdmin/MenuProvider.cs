using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using EPiServer.Security;
using EPiServer.Shell.Navigation;

namespace Geta.DdsAdmin
{
    [MenuProvider]
    public class MenuProvider : IMenuProvider
    {
        private const string RootMenuUri = "/Modules/Geta.DdsAdmin";

        #region IMenuProvider Members

        public IEnumerable<MenuItem> GetMenuItems()
        {
            var menuItems = new List<MenuItem> {};
            const string parentPath = MenuPaths.Global + "/geta";
            HttpContext context = HttpContext.Current;

            if (!Convert.ToBoolean(context.Items["GetaTopMenuIsSet"]))
            {
                var mainMenu = new SectionMenuItem("Geta", parentPath) { IsAvailable = CheckAccess };
                menuItems.Add(mainMenu);
                context.Items["GetaTopMenuIsSet"] = true;
            }

            var adminItem = new UrlMenuItem("DDS Admin", parentPath + "/dds_admin",
                                            RootMenuUri + "/Admin/Default.aspx")
                                {
                                    IsAvailable = CheckAccess
                                };
            menuItems.Add(adminItem);

            return menuItems;
        }

        #endregion

        protected bool CheckAccess(RequestContext requestContext)
        {
            if (PrincipalInfo.Current != null)
            {
                return PrincipalInfo.Current.HasPathAccess(RootMenuUri) || PrincipalInfo.HasAdminAccess;
            }

            return false;
        }
    }
}