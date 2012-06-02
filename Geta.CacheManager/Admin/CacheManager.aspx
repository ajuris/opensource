<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CacheManager.aspx.cs" Inherits="Geta.CacheManager.Admin.CacheManager" %>
<%@ Import Namespace="SquishIt.Framework" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Cache Manager</title>
        <link rel="stylesheet" type="text/css" href="/cms/Shell/ClientResources/ShellCore.css"/>
        <link rel="stylesheet" type="text/css" href="/cms/Shell/ClientResources/ShellCoreLightTheme.css"/>
        <link rel="stylesheet" type="text/css" href="/cms/Shell/ClientResources/EPi/Shell/Light/Shell.css"/>
        <link rel="stylesheet" type="text/css" href="/App_Themes/Default/Styles/ToolButton.css"/>
    </head>
    <body>
        <form id="form1" runat="server">
            <div class="epi-contentContainer epi-padding-large">
                <div class="epi-buttonDefault">
                    <span class="epi-cmsButton">
                        <input type="button" data-role="refresh-stats-button" class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Refresh" value="Refresh" />
                    </span>
                    <span class="epi-cmsButton">
                        <input type="button" data-role="clear-cache-button" class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Delete" value="Clear All Cache" />
                    </span>
                </div>
                <h1 class="EP-prefix">System Stats</h1>
                <table data-role="server-stats-table" class="epi-default">
                    <tbody>
                    </tbody>
                </table>
                <h1 class="EP-prefix">Cache List</h1>
                <div class="epi-buttonDefault">
                    <span class="epi-cmsButton">
                        <input type="button" data-role="delete-selected-cache-button" class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Delete" value="Delete Selected" />
                    </span>
                </div>
                <table data-role="cache-list-table" class="epi-default">
                    <thead>
                        <tr>
                            <th><input type="checkbox" id="cm-checkbox-select-all" /></th>
                            <th>Key</th>
                        </tr>
                    </thead>
                    <tbody>
                    </tbody>
                </table>
            </div>
        </form>
        <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"> </script>
        <script type="text/javascript"> window.jQuery || document.write('<script src="/Scripts/jquery-1.7.2.min.js><\/script>') </script>
        <script type="text/javascript" src="//cdnjs.cloudflare.com/ajax/libs/json2/20110223/json2.min.js"></script>
        <script type="text/javascript"> window.JSON || document.write('<script src="/Scripts/json2.min.js><\/script>') </script>
        <%= Bundle.JavaScript().Add("../Scripts/cachemanager.js").Render("../Scripts/cm_combined_#.js") %>
    </body>
</html>