<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Menu.aspx.cs" Inherits="Geta.DdsAdmin.Admin.Menu" MasterPageFile="../Temp.Master" %>
<%@ Import Namespace="Geta.DdsAdmin.Dds" %>

<asp:Content ContentPlaceHolderID="HeaderContentRegion" runat="server">
    <base target="ep_main" />
</asp:Content>

<asp:Content ContentPlaceHolderID="FullRegion" runat="server">
    <asp:Panel ID="tabView" runat="server" CssClass="epi-adminSidebar">
        <div class="epi-localNavigation">
            <div style="margin: 20px 0 0 20px;">
                <label for="txtListFilter">Filter: </label>
                <input type="text" id="txtListFilter" />
            </div>
            <ul>
                <li class="epi-navigation-standard epi-navigation-selected">
                    <asp:Repeater runat="server" ID="repStoreTypes">
                        <HeaderTemplate>
                            <ul>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <a name="pLink" class="epi-navigation-global_user_settings_shell_search " href='DdsAdmin.aspx?Store=<%# ((StoreInfo)Container.DataItem).Name %>'>
                                    <%# string.Format("{0} Rows:{1} Colums:{2}",
                                                      ((StoreInfo)Container.DataItem).Name,
                                                      ((StoreInfo)Container.DataItem).Rows,
                                                      ((StoreInfo)Container.DataItem).Columns.Count()+1) %>
                                </a>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                </li>
            </ul>
        </div>
        <script src="../Scripts/listfilter.js" type="text/javascript"> </script>
        <script type="text/javascript">
            $(document).ready(function () {
                $('#txtListFilter').listFilter({ listName: '.epi-navigation-standard.epi-navigation-selected' });
            });
        </script>
    </asp:Panel>
</asp:Content>