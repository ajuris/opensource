<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Geta.CacheManager.Admin.Default" MasterPageFile="../FrameworkTemp.Master" %>
<%@ Register TagPrefix="EPiServerShell" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>
<%@ Register TagPrefix="EPiServerUI" Assembly="EPiServer.UI" Namespace="EPiServer.UI.WebControls" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="FullRegion">
    <div class="epi-globalNavigationContainer">
        <EPiServerShell:ShellMenu runat="server" SelectionPath="/global/geta/cachemanager" Area="Geta" ID="Menu" />
    </div>
    <EPiServerUI:DynamicTable runat="server" NumberOfColumns="3" CellPadding="0" CellSpacing="0" ID="DynamicTable2" KeyName="EPAdminFramework">
        <EPiServerUI:DynamicRow runat="server" ID="DynamicRow3">
            <EPiServerUI:DynamicCell runat="server" style="height: 4.4em;" ID="DynamicCell5" Colspan="3">
                <!-- Do not remove this cell. It reserves space for the global navigation -->
            </EPiServerUI:DynamicCell>
        </EPiServerUI:DynamicRow>
        <EPiServerUI:DynamicRow runat="server" ID="DynamicRow4">
            <EPiServerUI:DynamicCell runat="server" Floating="True" ID="Dynamiccell6">
                <EPiServerUI:SystemIFrame runat="server" id="InfoFrame" SourceFile="CacheManager.aspx" Name="ep_main" />
            </EPiServerUI:DynamicCell>
        </EPiServerUI:DynamicRow>
    </EPiServerUI:DynamicTable>
</asp:Content>