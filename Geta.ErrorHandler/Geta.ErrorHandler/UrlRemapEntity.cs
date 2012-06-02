using System;
using EPiServer.Data.Dynamic;

namespace Geta.ErrorHandler
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class UrlRemapEntity
    {
        [EPiServerDataIndex]
        public Guid Id { get; set; }

        [EPiServerDataIndex]
        public string OldUrl { get; set; }

        public int PageId { get; set; }

        public string LanguageBranch { get; set; }
    }
}
