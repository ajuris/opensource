using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Web;
using EPiServer;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Web;
using System.Linq;
using log4net;
using Mediachase.Commerce.Assets;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace Geta.Commerce.AmazonS3.Modules
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
                    var url = Helpers.AmazonS3VirtualPathHelper.GetBaseUrl(providerSetting);
                    var path = e.Url.Path.Replace(virtualPath, string.Empty);

                    var pathSplit = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

                    var folders = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    folders.Remove(pathSplit.Last());

                    var currentFolder = FolderEntity.GetChildFolders(1).First(n => n.Name == folders.First().ToString(CultureInfo.InvariantCulture));
                    foreach (var folder in folders)
                    {
                        if (currentFolder.PrimaryKeyId.HasValue)
                        {
                            foreach (var child in FolderEntity.GetChildFolders(currentFolder.PrimaryKeyId.Value))
                            {
                                if (child.Name == folder)
                                {
                                    currentFolder = child;
                                }
                            }
                        }
                    }
                    if (currentFolder.PrimaryKeyId != null)
                    {
                        var folderElementEntities = FolderEntity.GetChildElements(currentFolder.PrimaryKeyId.Value);
                        var imagename = folderElementEntities.First(a => a.Name == pathSplit.Last().ToString(CultureInfo.InvariantCulture));
                        Helpers.AmazonS3VirtualPathHelper.SetFileToPublic(providerSetting, imagename.BlobUid.ToString());

                        url = UriSupport.Combine(url, imagename.BlobUid.ToString());
                    }

                    try
                    {
                        // Assuming url is the  url to the S3 object.
                        var buffer = new byte[1024 * 8]; // 8k buffer. 
                        var request = (HttpWebRequest)WebRequest.Create(url);
                        var response = request.GetResponse();
                        int bytesRead = 0;
                        HttpContext.Current.Response.ContentType = request.ContentType;

                        using (var responseStream = response.GetResponseStream())
                        {
                            while (responseStream != null && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                HttpContext.Current.Response.OutputStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                    catch(WebException exception)
                    {
                        if (exception.Status == WebExceptionStatus.ProtocolError)
                        {
                            HttpContext.Current.Response.StatusCode = 403;
                        }
                    }

                    HttpContext.Current.Response.End();
                }
            }
        }
    }
}