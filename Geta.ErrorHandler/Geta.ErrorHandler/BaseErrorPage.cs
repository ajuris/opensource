using System.Web;
using EPiServer;
using EPiServer.Web.PageExtensions;

namespace Geta.ErrorHandler
{
    public abstract class BaseErrorPage : TemplatePage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref = "BaseErrorPage" /> class.
        /// </summary>
        /// <remarks>
        ///     Available options must be provided for the base page in order to work with Server.Transfer
        /// </remarks>
        protected BaseErrorPage() : base(0, HttpContext.Current.Items["InErrorHandler"] == null ? 0 : 0 | ContextMenu.OptionFlag)
        {
        }
    }
}
