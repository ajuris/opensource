using System;
using System.IO;
using System.Reflection;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Web;
using Geta.ErrorHandler.Configuration;
using Geta.ErrorHandler.Redirect;
using log4net;

namespace Geta.ErrorHandler
{
    internal class ErrorPageRender
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly HttpContextBase context;

        public ErrorPageRender(HttpContextBase context)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.context = context;
        }

        public void Render()
        {
            // this method is called when there Server.GetLastError() returns null
            // assuming that there was unknown application error - code 500
            RenderSteps(500, null);
        }

        public void Render(Exception exception)
        {
            int statusCode;
            string newUrl = null;
            if(exception is HttpException)
            {
                var httpEx = exception as HttpException;
                statusCode = httpEx.GetHttpCode();

                if(Settings.IsOldNewUrlRemapperEnabled && statusCode == 404 && this.context.Request.Url != null)
                {
                    string fullUrl = this.context.Request.Url.ToString();
                    int startIndex = fullUrl.LastIndexOf(';');
                    string requestedUrl = fullUrl.Substring(startIndex + 1);
                    int endIndex = requestedUrl.LastIndexOf('?');
                    string absoluteUrl = endIndex != -1 ? requestedUrl.Substring(0, endIndex) : requestedUrl;
                    var uri = new Uri(absoluteUrl);

                    var mapper = new OldNewUrlMapper();
                    newUrl = mapper.GetNewUrl(uri.PathAndQuery);
                    statusCode = newUrl == null ? 404 : 301;
                }
            }
            else
            {
                if(exception is PageNotFoundException)
                {
                    statusCode = 404;
                }
                else if(exception is AccessDeniedException)
                {
                    statusCode = 401;
                }
                else if(exception is FileNotFoundException)
                {
                    statusCode = 404;
                }
                else
                {
                    statusCode = 500;
                }
            }

            RenderSteps(statusCode, newUrl);
        }

        private void EndResponse(int httpErrorCode, string newUrl)
        {
            this.context.Response.TrySkipIisCustomErrors = true;
            this.context.Response.StatusCode = httpErrorCode;
            if(httpErrorCode == 301)
            {
                this.context.Response.Status = "301 Moved Permanently";
                if(newUrl != null)
                {
                    this.context.Response.AddHeader("Location", newUrl);
                }
            }

            this.context.Response.Flush();
            this.context.Response.End();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private void LogError(string message)
        {
            if(logger.IsErrorEnabled)
            {
                logger.Error(message);
            }
        }

        private void RenderSteps(int statusCode, string newUrl)
        {
            StartResponse();
            TransferToFile(statusCode);
            EndResponse(statusCode, newUrl);
        }

        private void StartResponse()
        {
            this.context.Response.Clear();
        }

        private void Transfer(PageReference reference, int httpErrorCode)
        {
            var page = DataFactory.Instance.GetPage(reference, new LanguageSelector(ContentLanguage.PreferredCulture.ToString()))
                       ?? DataFactory.Instance.GetPage(reference);

            if(httpErrorCode == 500)
            {
                this.context.Server.TransferRequest(page.LinkURL);
            }
            else
            {
                this.context.Server.Execute(page.LinkURL);
            }
        }

        private void TransferToFile(int statusCode)
        {
            try
            {
                HttpContext.Current.Items["InErrorHandler"] = true;

                // get error page reference from start page
                var startPage = DataFactory.Instance.GetPage(PageReference.StartPage);
                if(startPage != null)
                {
                    var reference = startPage[string.Format("Error{0}PageReference", statusCode)];
                    if(reference != null && reference is PageReference)
                    {
                        Transfer((PageReference)reference, statusCode);
                        return;
                    }
                }

                // startPage not found or Error{0}PageReference not found
                // get error page reference by friendly URL
                object pageReference;
                if(new FriendlyUrlRewriteProvider().ConvertToInternal(new UrlBuilder(string.Format("/{0}", statusCode)), out pageReference))
                {
                    Transfer((PageReference)pageReference, statusCode);
                    return;
                }

                // if we get this far - use static error page as fallback to display user friendly error
                this.context.Server.Execute(string.Format("/modules/Geta/ErrorHandler/{0}.html", statusCode));
            }
            catch(Exception ex)
            {
                LogError("Unknown exception occurred. " + ex);
                throw;
            }
        }
    }
}
