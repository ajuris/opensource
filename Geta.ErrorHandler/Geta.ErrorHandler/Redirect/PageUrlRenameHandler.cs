using System;
using System.Linq;
using System.Reflection;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.PlugIn;
using Geta.ErrorHandler.Configuration;
using log4net;

namespace Geta.ErrorHandler.Redirect
{
    public class PageUrlRenameHandler : PlugInAttribute
    {
        public const string OldUrl = "Geta-ErrorHandler-OldUrl";

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Start()
        {
            DataFactory.Instance.PublishingPage += OnPublishingPage;
        }

        protected static void OnPublishingPage(object sender, PageEventArgs e)
        {
            // check if old URL remapping is enabled
            if(!Settings.IsOldNewUrlRemapperEnabled)
            {
                return;
            }

            var oldUrl = new UrlBuilder(e.Page.LinkURL);
            if(Global.UrlRewriteProvider.ConvertToExternal(oldUrl, e.Page.PageLink, Encoding.UTF8))
            {
                if(!oldUrl.Uri.ToString().EndsWith(e.Page.URLSegment + "/"))
                {
                    logger.Debug(string.Format("Old Url=[{0}] does not match with new one=[{1}]", oldUrl.Uri, e.Page.URLSegment));

                    // process page and iterate through all child pages
                    SaveOldUrlRecursive(e.PageLink);
                }
            }
        }

        private static bool IsSystemPage(PageData page)
        {
            return page.PageLink.CompareToIgnoreWorkID(PageReference.RootPage)
                   || page.PageLink.CompareToIgnoreWorkID(PageReference.WasteBasket);
        }

        private static void SaveOldUrlRecursive(PageReference pageId)
        {
            var languageBranches = DataFactory.Instance.GetLanguageBranches(pageId);
            foreach(var page in languageBranches)
            {
                try
                {
                    if(!IsSystemPage(page))
                    {
                        // some how noticed that even iteration via language branches
                        // page url gets back always with epslanguage=en query string
                        // so adding correct language selector manually - seems to be working
                        var languageSupportedUrl = UriSupport.AddLanguageSelection(page.LinkURL, page.LanguageBranch);
                        var oldUrl = new UrlBuilder(languageSupportedUrl);

                        if(Global.UrlRewriteProvider.ConvertToExternal(oldUrl, page.PageLink, Encoding.UTF8))
                        {
                            // create new DDS entry for this particular page
                            var entry = new UrlRemapEntity
                                        {
                                            Id = Guid.NewGuid(), 
                                            OldUrl = oldUrl.Uri.ToString(), 
                                            PageId = pageId.ID, 
                                            LanguageBranch = page.LanguageBranch
                                        };

                            var store = typeof(UrlRemapEntity).GetStore();

                            // check for duplicates
                            var foundItem =
                                store.Items<UrlRemapEntity>().FirstOrDefault(
                                                                             i =>
                                                                             i.OldUrl == entry.OldUrl && i.PageId == entry.PageId
                                                                             && i.LanguageBranch == entry.LanguageBranch);
                            if(foundItem == null)
                            {
                                store.Save(entry);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger.Error("Can't set URL for page '" + page.PageLink + "' on language '" + page.LanguageBranch + "' because: ", ex);
                }
            }

            foreach(var pageData in DataFactory.Instance.GetChildren(pageId, LanguageSelector.AutoDetect(true)))
            {
                SaveOldUrlRecursive(pageData.PageLink);
            }
        }
    }
}
