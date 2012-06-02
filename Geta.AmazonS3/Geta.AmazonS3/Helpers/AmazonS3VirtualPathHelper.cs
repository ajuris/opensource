using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using EPiServer.Web.Hosting;
using Geta.AmazonS3.Hosting;

namespace Geta.AmazonS3.Helpers
{
    public static class AmazonS3VirtualPathHelper
    {
        private static readonly object CacheLockObject = new object();

         public static IEnumerable<ProviderSettings> GetAllSettings()
         {
             string cacheKey = "AmazonS3Providers";

             var allAmazonS3ProviderSettings = HttpRuntime.Cache[cacheKey] as List<ProviderSettings>;

             if (allAmazonS3ProviderSettings == null)
             {
                 lock (CacheLockObject)
                 {
                     allAmazonS3ProviderSettings = HttpRuntime.Cache[cacheKey] as List<ProviderSettings>;

                     if (allAmazonS3ProviderSettings == null)
                     {
                         allAmazonS3ProviderSettings = VirtualPathHandler.Instance.VirtualPathProviders.Where(p => p.Key.GetType() == typeof (AmazonS3VirtualPathProvider)).Select(p => p.Value).ToList();

                         HttpRuntime.Cache.Insert(cacheKey, allAmazonS3ProviderSettings, null, DateTime.MaxValue, TimeSpan.Zero);
                     }
                 }
             }

             return allAmazonS3ProviderSettings;
         }

        public static string GetVirtualPath(ProviderSettings providerSettings)
        {
            string virtualPath = providerSettings.Parameters["virtualPath"];

            virtualPath = VirtualPathUtility.ToAbsolute(virtualPath);

            return virtualPath;
        }

        public static string GetHostName(ProviderSettings providerSettings)
        {
            return providerSettings.Parameters["hostName"];
        }

        public static string GetScheme(ProviderSettings providerSettings)
        {
            return providerSettings.Parameters["scheme"] ?? "http://";
        }

        public static string GetBaseUrl(ProviderSettings providerSettings)
        {
            string hostName = GetHostName(providerSettings);

            string scheme = GetScheme(providerSettings);

            return VirtualPathUtility.AppendTrailingSlash(scheme + hostName);
        }
    }
}