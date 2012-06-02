using System;
using System.Web;

namespace Geta.ErrorHandler
{
    /// <summary>
    ///     Error module is used to handle Asp.Net runtime errors (accessing non-existing files, 500 runtime errors etc).
    /// </summary>
    public class ErrorHandlerModule : IHttpModule
    {
        void IHttpModule.Dispose()
        {
            this.Dispose();
        }

        void IHttpModule.Init(HttpApplication context)
        {
            this.Init(context);
        }

        public void Dispose()
        {
        }

        protected void Init(HttpApplication context)
        {
            context.Error += this.ProcessErrorRequest;
        }

        private void ProcessErrorRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                return;
            }

            var processor = new ErrorHandlerProcessor(new HttpContextWrapper(context));
            processor.Process();
        }
    }
}
