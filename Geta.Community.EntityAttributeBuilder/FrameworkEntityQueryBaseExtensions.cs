using System;
using EPiServer.Common.Queries;

namespace Geta.Community.EntityAttributeBuilder
{
    public static class FrameworkEntityQueryBaseExtensions
    {
        public static MetadataFrameworkEntityQuery<T> AsMetadataQuery<T>(this FrameworkEntityQueryBase query) where T : class
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return new MetadataFrameworkEntityQuery<T>(query);
        }
    }
}
