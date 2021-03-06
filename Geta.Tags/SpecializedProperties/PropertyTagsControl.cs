﻿using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Geta.Tags.Implementations;
using Geta.Tags.Interfaces;

namespace Geta.Tags.SpecializedProperties
{
    public class PropertyTagsControl : EPiServer.Web.PropertyControls.PropertyTextBoxControlBase
    {
        public ITagService TagService { get; set; }

        public PropertyTagsControl()
        {
            this.TagService = new TagService();
        }

        private const string ClientScriptFormat = @"$(document).ready(function(){{
            $('#{0}').tagsInput({{
                autocomplete_url:'{1}'
            }});
        }});";

        public override bool SupportsOnPageEdit
        {
            get
            {
                return false;
            }
        }

        protected TextBox TextBox;

        public override void CreateEditControls()
        {
            TextBox = new TextBox();

            this.RegisterClientResources();

            if (PropertyData != null)
            {
                this.TextBox.Text = PropertyData.ToString();
            }
            
            ApplyControlAttributes(TextBox);
            Controls.Add(TextBox);

            if (!Page.ClientScript.IsClientScriptBlockRegistered(GetType(), ClientID))
            {
                Page.ClientScript.RegisterClientScriptBlock(GetType(), ClientID, string.Format(ClientScriptFormat, TextBox.ClientID, ResolveUrl("~/Geta/Tags/TagNameLookup.ashx")), true);
            }
        }

        private void RegisterClientResources()
        {
            // jQuery UI depends on this version
            if (!Page.ClientScript.IsClientScriptIncludeRegistered(GetType(), "jquery"))
            {
                Page.ClientScript.RegisterClientScriptInclude(GetType(), "jquery", ResolveUrl("~/Geta/Tags/Resources/jquery-1.6.4.min.js"));
            }

            if (!Page.ClientScript.IsClientScriptIncludeRegistered(GetType(), "jqueryui-autocomplete"))
            {
                Page.ClientScript.RegisterClientScriptInclude(GetType(), "jqueryui-autocomplete", ResolveUrl("~/Geta/Tags/Resources/jquery-ui-1.8.16.custom.min.js"));
            }

            if (!Page.ClientScript.IsClientScriptIncludeRegistered(GetType(), "tagsinput"))
            {
                Page.ClientScript.RegisterClientScriptInclude(GetType(), "tagsinput", ResolveUrl("~/Geta/Tags/Resources/jquery.tagsinput.min.js"));
            }

            Page.Header.Controls.Add(CreateCssLink(ResolveUrl("~/Geta/Tags/Resources/jquery.tagsinput.css"), "screen"));
            Page.Header.Controls.Add(CreateCssLink(ResolveUrl("~/Geta/Tags/Resources/jquery-ui-1.8.16.custom.css"), "screen"));
        }

        public static HtmlLink CreateCssLink(string cssFilePath, string media)
        {
            var link = new HtmlLink();
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            link.Href = link.ResolveUrl(cssFilePath);
            if (string.IsNullOrEmpty(media))
            {
                media = "all";
            }

            link.Attributes.Add("media", media);
            return link;
        }

        public override void ApplyEditChanges()
        {
            string tags = this.TextBox.Text;

            if (!string.IsNullOrEmpty(tags))
            {
                foreach (string name in tags.Split(','))
                {
                    this.TagService.Save(CurrentPage.PageGuid, name);
                }
            }

            this.SetValue(tags);
        }
    }
}