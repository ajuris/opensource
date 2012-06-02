using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web;
using EPiServer;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Web;
using System.Linq;
using log4net;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace Geta.AmazonS3.Modules
{
    [InitializableModule]
    [ModuleDependency((typeof(InitializationModule)))]
    public class AmazonS3Module : IInitializableModule
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Initialize(InitializationEngine context)
        {
            UrlRewriteModuleBase.HttpRewriteInit += HttpRewriteInit;
        }

        public void Preload(string[] parameters)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
            UrlRewriteModuleBase.HttpRewriteInit -= HttpRewriteInit;
        }

        private static void HttpRewriteInit(object sender, UrlRewriteEventArgs e)
        {
            var urlRewriteModule = (UrlRewriteModule)sender;
            urlRewriteModule.HttpRewritingToInternal += UrlRewriteModuleHttpRewritingToInternal;
            HtmlRewriteToExternal.HtmlRewriteInit += HtmlRewriteToExternalHtmlRewriteInit; 
        }

        private static void HtmlRewriteToExternalHtmlRewriteInit(object sender, HtmlRewriteEventArgs e)
        {
            e.RewritePipe.HtmlRewriteUrl += RewritePipeHtmlRewriteUrl;
        }

        // Rewrites before displaying HTML code
        private static void RewritePipeHtmlRewriteUrl(object sender, HtmlRewriteEventArgs e)
        {
            try
            {
                IEnumerable<ProviderSettings> providerSettings = Helpers.AmazonS3VirtualPathHelper.GetAllSettings();

                if (!providerSettings.Any())
                {
                    return;
                }

                foreach (ProviderSettings providerSetting in providerSettings)
                {
                    string virtualPath = Helpers.AmazonS3VirtualPathHelper.GetVirtualPath(providerSetting);

                    if (e.Value.StartsWith(virtualPath))
                    {
                        string url = Helpers.AmazonS3VirtualPathHelper.GetBaseUrl(providerSetting);

                        e.ValueBuilder.Replace(virtualPath, string.Empty);
                        e.ValueBuilder.Insert(0, url);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                // Important to never throw an exception in the friendly URL handling.
            }
        } 

        private static void UrlRewriteModuleHttpRewritingToInternal(object sender, UrlRewriteEventArgs e)
        {
            IEnumerable<ProviderSettings> providerSettings = Helpers.AmazonS3VirtualPathHelper.GetAllSettings();

            if (!providerSettings.Any())
            {
                return;
            }

            foreach (ProviderSettings providerSetting in providerSettings)
            {
                string virtualPath = Helpers.AmazonS3VirtualPathHelper.GetVirtualPath(providerSetting);

                if (e.Url.Path.StartsWith(virtualPath))
                {
                    string url = Helpers.AmazonS3VirtualPathHelper.GetBaseUrl(providerSetting);

                    string path = e.Url.Path.Replace(virtualPath, string.Empty);

                    url = UriSupport.Combine(url, path);

                    HttpContext.Current.Response.Redirect(url, true);
                }
            }
        }
    }
}