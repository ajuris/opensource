using System.Linq;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;

namespace Geta.ErrorHandler.Redirect
{
    public class OldNewUrlMapper
    {
        public string GetNewUrl(string oldUrl)
        {
            if(!oldUrl.EndsWith("/"))
            {
                oldUrl += "/";
            }

            // lookup Url in DDS
            var store = typeof(UrlRemapEntity).GetStore();
            var foundItem = store.Items<UrlRemapEntity>().FirstOrDefault(i => i.OldUrl.Equals(oldUrl));

            if(foundItem != null)
            {
                var reference = new PageReference(foundItem.PageId);
                var pageData = DataFactory.Instance.GetPage(reference);
                pageData = pageData.GetPageLanguage(foundItem.LanguageBranch);
                var builder = new UrlBuilder(UriSupport.AddLanguageSelection(pageData.LinkURL, pageData.LanguageBranch));
                
                if(Global.UrlRewriteProvider.ConvertToExternal(builder, pageData.PageLink, Encoding.UTF8))
                {
                    return builder.Uri.ToString();
                }
            }

            return null;
        }
    }
}
