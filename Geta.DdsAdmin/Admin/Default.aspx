<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Geta.DdsAdmin.Admin.Default" MasterPageFile="../FrameworkTemp.Master"%>
<%@ Register TagPrefix="sc" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="FullRegion">
    <div class="epi-globalNavigationContainer">
        <sc:ShellMenu runat="server" SelectionPath="/global/geta/dds_admin" Area="Geta" ID="Menu" />
    </div>
    <EPiServerUI:DynamicTable runat="server" NumberOfColumns="3" CellPadding="0" CellSpacing="0" ID="DynamicTable2" KeyName="EPAdminFramework">
        <EPiServerUI:DynamicRow runat="server" ID="DynamicRow3">
            <EPiServerUI:DynamicCell runat="server" style="height: 4.4em;" ID="DynamicCell5" Colspan="3">
            <!-- Do not remove this cell. It reserves space for the global navigation -->
            </EPiServerUI:DynamicCell>
        </EPiServerUI:DynamicRow>
        <EPiServerUI:DynamicRow runat="server" ID="DynamicRow4">
            <EPiServerUI:DynamicCell runat="server" Width="270px" ID="Dynamiccell1">
                <EPiServerUI:DynamicTable runat="server" NumberOfColumns="1" CellPadding="0" CellSpacing="0" ID="Dynamictable1">
                    <EPiServerUI:DynamicRow runat="server" ID="Dynamicrow2" NAME="Dynamicrow2">
                        <EPiServerUI:DynamicCell runat="server" ID="Dynamiccell3">
                            <EPiServerUI:SystemIFrame runat="server" id="AdminMenu" SourceFile="Menu.aspx" Name="AdminMenu" IsScrollingEnabled="True" />
                        </EPiServerUI:DynamicCell>
                    </EPiServerUI:DynamicRow>
                    <EPiServerUI:DynamicRow style="height:23px;" runat="server" ID="Dynamicrow5">
                        <EPiServerUI:DynamicCell runat="server" ID="Dynamiccell4" CssClass="epifadedbackground" style="height:23px">
                            <asp:Panel runat="server" CssClass="epicontentarea" ID="Panel1">
                                <EPiServerUI:LicenseInfo runat="server" ID="Licenseinfo1" />
                            </asp:Panel>
                        </EPiServerUI:DynamicCell>
                    </EPiServerUI:DynamicRow>
                </EPiServerUI:DynamicTable>
            </EPiServerUI:DynamicCell>
            <EPiServerUI:DynamicResizeCell Width="10" CssClass="EPEdit-CustomDrag" KeyName="ResizeCell1">
            </EPiServerUI:DynamicResizeCell>
            <EPiServerUI:DynamicCell runat="server" Floating="True" ID="Dynamiccell6">
                <EPiServerUI:SystemIFrame runat="server" id="InfoFrame" SourceFile="DdsAdmin.aspx" Name="ep_main" />
            </EPiServerUI:DynamicCell>
        </EPiServerUI:DynamicRow>
    </EPiServerUI:DynamicTable>
</asp:Content>
