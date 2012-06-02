using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using Geta.Tags.Interfaces;
using Geta.Tags.Models;

namespace Geta.Tags.Implementations
{
    using System;

    using Geta.Tags.Helpers;

    public class TagEngine : ITagEngine
    {
        private readonly ITagService _tagService;

        public TagEngine() : this(new TagService())
        {
        }

        public TagEngine(ITagService tagService)
        {
            this._tagService = tagService;
        }

        public PageDataCollection GetPagesByTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return null;
            }

            return this.GetPagesByTag(this._tagService.GetTagByName(tagName));
        }

        public PageDataCollection GetPagesByTag(Tag tag)
        {
            if (tag == null)
            {
                return null;
            }

            var pageLinks = new List<Guid>();

            if (tag.PermanentLinks == null)
            {
                var tempTerm = this._tagService.GetTagByName(tag.Name);

                if (tempTerm != null)
                {
                    pageLinks = tempTerm.PermanentLinks.ToList();
                }
            }
            else
            {
                pageLinks = tag.PermanentLinks.ToList();
            }

            var pages = new PageDataCollection();

            foreach (Guid pageGuid in pageLinks)
            {
                pages.Add(DataFactory.Instance.GetPage(TagsHelper.GetPageReference(pageGuid)));
            }

            return pages;
        }

        public PageDataCollection GetPagesByTag(string tagName, PageReference rootPageReference)
        {
            return this.GetPagesByTag(this._tagService.GetTagByName(tagName));
        }

        public PageDataCollection GetPagesByTag(Tag tag, PageReference rootPageReference)
        {
            if (tag == null || tag.PermanentLinks == null)
            {
                return null;
            }

            IList<PageReference> descendantPageReferences = DataFactory.Instance.GetDescendents(rootPageReference);

            if (descendantPageReferences == null || descendantPageReferences.Count < 1)
            {
                return null;
            }

            var pages = new PageDataCollection();

            foreach (Guid pageGuid in tag.PermanentLinks)
            {
                var pageReference = TagsHelper.GetPageReference(pageGuid);

                if (descendantPageReferences.Where(p => p.ID == pageReference.ID).FirstOrDefault() != null)
                {
                    pages.Add(DataFactory.Instance.GetPage(pageReference));
                }
            }

            return pages;
        }

        public IEnumerable<PageReference> GetPageReferencesByTags(string tagNames)
        {
            var pageLinks = new List<Guid>();

            foreach (string tagName in tagNames.Split(','))
            {
                Tag tags = this._tagService.GetTagByName(tagName);

                if (tags != null && tags.PermanentLinks != null)
                {
                    pageLinks.AddRange(tags.PermanentLinks);
                }
            }

            return pageLinks.Select(TagsHelper.GetPageReference);
        }

        public IEnumerable<PageReference> GetPageReferencesByTags(IEnumerable<Tag> tags)
        {
            var pageGuids = new List<Guid>();

            foreach (Tag tag in tags)
            {
                pageGuids.AddRange(tag.PermanentLinks);
            }

            return pageGuids.Select(TagsHelper.GetPageReference);
        }

        public IEnumerable<PageReference> GetPageReferencesByTags(string tagNames, PageReference rootPageReference)
        {
            IList<Tag> tags = new List<Tag>();

            foreach (string tagName in tagNames.Split(','))
            {
                tags.Add(this._tagService.GetTagByName(tagName));
            }

            return this.GetPageReferencesByTags(tags, rootPageReference);
        }

        public IEnumerable<PageReference> GetPageReferencesByTags(IEnumerable<Tag> tags, PageReference rootPageReference)
        {
            if (tags == null || PageReference.IsNullOrEmpty(rootPageReference))
            {
                return null;
            }

            IList<PageReference> descendantPageReferences = DataFactory.Instance.GetDescendents(rootPageReference);

            if (descendantPageReferences == null || descendantPageReferences.Count < 1)
            {
                return null;
            }

            var matches = new List<PageReference>();

            foreach (Tag tag in tags)
            {
                if (tag == null || tag.PermanentLinks == null)
                {
                    continue;
                }

                foreach (Guid pageGuid in tag.PermanentLinks)
                {
                    var pageReference = TagsHelper.GetPageReference(pageGuid);

                    if (descendantPageReferences.Where(p => p.ID == pageReference.ID).FirstOrDefault() != null)
                    {
                        matches.Add(pageReference);
                    }
                }
            }

            return new PageReferenceCollection(matches.Distinct());
        }
    }
}