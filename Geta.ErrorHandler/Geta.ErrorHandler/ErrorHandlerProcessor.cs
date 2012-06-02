using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using log4net;

namespace Geta.ErrorHandler
{
    /// <summary>
    ///     Processing unit that will handle and render error page depending on status code.
    /// </summary>
    internal class ErrorHandlerProcessor
    {
        private static readonly List<string> ignoredResourceExtensions = new List<string> { "jpg", "gif", "png", "bmp", "css", "js", "ico", "swf" };

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly HttpContextBase context;

        public ErrorHandlerProcessor(HttpContextBase context)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.context = context;
        }

        public string GetOriginalRequestedFile(Uri requestUrl)
        {
            var fullPath = requestUrl.ToString();
            string[] requestQueryStrings = fullPath.Split(';');

            if(requestQueryStrings.Length > 1)
            {
                return requestQueryStrings[1];
            }

            return fullPath;
        }

        public bool IsResourceFile(Uri requestUrl)
        {
            var extension = GetOriginalRequestedFile(requestUrl);
            var extPos = extension.LastIndexOf('.');
            if(extPos > 0)
            {
                extension = extension.Substring(extPos + 1);
                if(ignoredResourceExtensions.Contains(extension))
                {
                    return true;
                }
            }

            return false;
        }

        public void Process()
        {
            var requestUrl = this.context.Request.Url;
            if(IsResourceFile(requestUrl))
            {
                if(requestUrl != null)
                {
                    LogDebug("Requested file (`" + requestUrl.AbsolutePath + "') is in ignored resource list.");
                }

                return;
            }

            var render = new ErrorPageRender(this.context);
            Exception lastException;
            try
            {
                lastException = this.context.Server.GetLastError();
                if(lastException == null)
                {
                    // if last error is null that means that error probably has occurred in static file handler
                    // (requesting file that does not exist)
                    // just render ordinary static page
                    render.Render();
                    return;
                }
            }
            catch(Exception ex)
            {
                LogError("Unknown exception occurred. " + ex);
                throw;
            }

            var innerEx = lastException.GetBaseException();
            LogDebug(string.Format("Rendering view for exception - {0}. More info: {1}", innerEx.GetType(), innerEx));

            render.Render(innerEx);
        }

        private void LogDebug(string message)
        {
            if(logger.IsDebugEnabled)
            {
                logger.Debug(message);
            }
        }

        private void LogError(string message)
        {
            if(logger.IsErrorEnabled)
            {
                logger.Error(message);
            }
        }
    }
}
