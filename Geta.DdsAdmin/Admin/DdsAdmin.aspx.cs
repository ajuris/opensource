using System;
using System.Linq;
using EPiServer.Data.Dynamic;
using EPiServer.Security;
using EPiServer.UI;
using Geta.DdsAdmin.Dds;

namespace Geta.DdsAdmin.Admin
{
    public partial class DdsAdmin : SystemPageBase
    {
        protected string CurrentStoreName { get; set; }
        protected StoreInfo Store { get; set; }
        protected string Columns { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!PrincipalInfo.HasAdminAccess)
            {
                AccessDenied();
            }

            CurrentStoreName = Request.QueryString["Store"];

            if (!IsPostBack)
            {
                hdivStoreTypeDoesntExist.Visible = false;

                if (string.IsNullOrEmpty(CurrentStoreName))
                {
                    hdivNoStoreTypeSelected.Visible = true;
                    hdivStoreTypeSelected.Visible = false;
                    return;
                }

                hdivNoStoreTypeSelected.Visible = false;
                hdivStoreTypeSelected.Visible = true;

                var explorer = new Store();

                Store = explorer.Explore().FirstOrDefault(s => s.Name == CurrentStoreName);

                if (Store == null)
                {
                    hdivNoStoreTypeSelected.Visible = false;
                    hdivStoreTypeSelected.Visible = false;
                    hdivStoreTypeDoesntExist.Visible = true;
                    return;
                }

                repColumnsHeader.DataSource = Store.Columns;
                repForm.DataSource = Store.Columns;
                repColumnsHeader.DataBind();
                repForm.DataBind();
                Columns = GenerateColumnData(Store);
            }
        }

        private string GenerateColumnData(StoreInfo store)
        {
            // first column is Guid id so its always read only
            string result = @"null";

            // get other columns
            foreach (PropertyMap column in Store.Columns)
            {
                if (column.PropertyType == typeof (bool))
                {
                    result += @", {type: 'select', onblur: 'submit', data: ""{'True':'True', 'False':'False'}""}";
                    continue;
                }
                if (column.PropertyType == typeof (int))
                {
                    result += @", {cssclass: 'number'}";
                    continue;
                }
                result += @", {}"; // empty definition for to be treated as string
            }
            return result;
        }
    }
}