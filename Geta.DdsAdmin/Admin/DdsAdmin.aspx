<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DdsAdmin.aspx.cs" Inherits="Geta.DdsAdmin.Admin.DdsAdmin"%>
<%@ Import Namespace="EPiServer.Data.Dynamic" %>
<%@ Import Namespace="SquishIt.Framework" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>
    <head runat="server">
        <meta http-equiv="Content-type" content="text/html; charset=UTF-8" />
        <title><%=CurrentStoreName%></title>
        <asp:PlaceHolder runat="server">
            <%= Bundle.Css()
                .Add("~/Content/DataTables-1.9.1/media/css/demo_page.css")
                .Add("~/Content/DataTables-1.9.1/media/css/demo_table.css")
                .Add("~/Content/DataTables-1.9.1/media/css/demo_table_jui.css")
                .Render("~/Content/DataTables-1.9.1/combined-#.css")
            %>
        </asp:PlaceHolder>
        <link type="text/css" rel="stylesheet" href="../../../Content/themes/jqueryui-custom/jquery-ui-1.8.20.custom.css"/>
        <style type="text/css" media="screen">
            .dataTables_info { padding-top: 0; }
            .dataTables_paginate { padding-top: 0; }
            .css_right { float: right; }
            #example_wrapper .fg-toolbar { font-size: 0.8em }
            #theme_links span {
                float: left;
                padding: 2px 10px;
            }
            .ui-widget { font-size: 0.8em; }
            #hdivNoStoreTypeSelected,
            #hdivStoreTypeDoesntExist,
            #hdivStoreTypeSelected {
                padding-left: 10px;
                padding-right: 10px;
            }
        </style>
        <script src="../../../Scripts/DataTables-1.9.1/media/js/complete.js" type="text/javascript"> </script>
        <script src="../../../Scripts/jquery-1.7.2.min.js" type="text/javascript"> </script>
        <script src="../../../Scripts/DataTables-1.9.1/media/js/jquery.dataTables.min.js" type="text/javascript"> </script>
        <script src="../../../Scripts/DataTables-1.9.1/media/js/jquery.dataTables.editable.js" type="text/javascript"> </script>
        <script src="../../../Scripts/DataTables-1.9.1/media/js/jquery.jeditable.js" type="text/javascript"> </script>
        <script src="../../../Scripts/jquery-ui-1.8.19.min.js" type="text/javascript"> </script>
        <script src="../../../Scripts/jquery.validate.min.js" type="text/javascript"> </script>
    </head>
    <body>
        <div runat="server" id="hdivNoStoreTypeSelected">
            <h3>No Store Type selected</h3>
        </div>
        
        <div runat="server" id="hdivStoreTypeDoesntExist">
            <h3>Selected Store Type does not exist</h3>
        </div>

        <div runat="server" id="hdivStoreTypeSelected">
            <form id="formAddNewRow" action="#" title="Add new <%=CurrentStoreName%>">
                <asp:repeater runat="server" ID="repForm" >
                    <HeaderTemplate>
                        <input type="text" name="form_Id" id="form_Id" rel="0" readonly="readonly" style="display: none" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <label for="form_<%#((PropertyMap)Container.DataItem).PropertyName%>">
                            <%#((PropertyMap)Container.DataItem).PropertyName%>
                        </label>
                        <input type="text" name="form_<%#((PropertyMap)Container.DataItem).PropertyName%>"
                               id="form_<%#((PropertyMap)Container.DataItem).PropertyName%>" rel="<%#Container.ItemIndex + 1%>" />
                    </ItemTemplate>
                    <SeparatorTemplate>
                        <br />
                    </SeparatorTemplate>
                </asp:repeater>
            </form>

            <h3>Selected Store Type: <%=CurrentStoreName%></h3>

            <table cellpadding="0" cellspacing="0" border="0" class="display" id="storeItems">
                <thead>
                    <tr>
                        <th>Id</th>
                        <asp:Repeater runat="server" ID="repColumnsHeader">
                            <ItemTemplate>
                                <th><%#((PropertyMap)Container.DataItem).PropertyName%></th>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td colspan="2" class="dataTables_empty">Loading data from server</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <script type="text/javascript" charset="utf-8">
            $(document).ready(function() {
                $('#storeItems').dataTable({
                        bJQueryUI: true,
                        bProcessing: true,
                        bServerSide: true,
                        sPaginationType: "full_numbers",
                        sAjaxSource: "Data.ashx?operation=read&store=<%=CurrentStoreName%>"
                    }).makeEditable({
                            sUpdateURL: "Data.ashx?operation=update&store=<%=CurrentStoreName%>",
                            sAddURL: "Data.ashx?operation=create&store=<%=CurrentStoreName%>",
                            sAddHttpMethod: "POST",
                            sDeleteHttpMethod: "POST",
                            sDeleteURL: "Data.ashx?operation=delete&store=<%=CurrentStoreName%>",
                            oAddNewRowButtonOptions: {
                                label: "Add...",
                                icons: { primary: 'ui-icon-plus' }
                            },
                            oDeleteRowButtonOptions: {
                                label: "Remove",
                                icons: { primary: 'ui-icon-trash' }
                            },
                            oAddNewRowFormOptions: {
                                title: 'Add a new <%=CurrentStoreName%>',
                                show: "blind",
                                hide: "explode",
                                modal: false
                            },
                            sAddDeleteToolbarSelector: ".dataTables_length",
                            sAddNewRowFormId: "formAddNewRow",
                            aoColumns: [<%= Columns %>]
                        });
            });
        </script>
    </body>
</html>