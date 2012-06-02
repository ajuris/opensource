using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using EPiServer.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Geta.CacheManager.Admin
{
    /// <summary>
    /// Handler class for Cache Manager. This is where requests are processed.
    /// </summary>
    public class CacheManagerController : IHttpHandler
    {
        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!PrincipalInfo.HasAdminAccess)
                {
                    context.Response.StatusCode = 401;
                    context.Response.End();
                    return;
                }

                string cmd = context.Request.QueryString["cmd"];
                if (context.Request.RequestType == "POST")
                {
                    cmd = context.Request["cmd"];
                }

                if (cmd == "getinfo")
                {
                    GetCacheInfo(context);
                }
                else if (cmd == "clearcache")
                {
                    ClearCache(context);
                }
                else if (cmd == "getcachelist")
                {
                    GetCacheList(context);
                }
                else if (cmd == "deleteselectedcache")
                {
                    DeleteSelectedCache(context, JArray.Parse(context.Request["cachelist"]));
                }
            }
            catch (NotImplementedException ex)
            {
                context.Response.StatusCode = 501;
                context.Response.Write(JsonConvert.SerializeObject(ex.Message));
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        #endregion

        protected void GetCacheInfo(HttpContext context)
        {
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                var result = new
                                 {
                                     Environment.MachineName,
                                     AvailableMemory = GetAvailableMemory(),
                                     current.Request.ApplicationPath,
                                     AppCacheEntries = current.Cache.Count.ToString(CultureInfo.InvariantCulture)
                                 };
                context.Response.Write(JsonConvert.SerializeObject(result));
            }
        }

        protected string GetAvailableMemory()
        {
            PerformanceCounter counter = null;

            try
            {
                counter = new PerformanceCounter("Memory", "Available MBytes");
                return counter.NextValue().ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return "Error";
            }
            finally
            {
                if (counter != null)
                {
                    counter.Dispose();
                }
            }
        }

        protected void ClearCache(HttpContext context)
        {
            try
            {
                Common.ClearCache();
                context.Response.Write("true");
            }
            catch (Exception e)
            {
                context.Response.Write("false");
            }
        }

        protected void GetCacheList(HttpContext context)
        {
            try
            {
                ArrayList result = Common.GetCacheList();
                context.Response.Write(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                context.Response.Write("false");
            }
        }

        protected void DeleteSelectedCache(HttpContext context, JArray cacheList)
        {
            try
            {
                foreach (JToken item in cacheList)
                {
                    Common.DeleteCacheItem(item["value"].ToString());
                }
                context.Response.Write("true");
            }
            catch (Exception e)
            {
                context.Response.Write("false");
            }
        }
    }
}