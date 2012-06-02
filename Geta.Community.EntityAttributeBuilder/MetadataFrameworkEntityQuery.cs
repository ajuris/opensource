using System;
using System.Linq.Expressions;
using EPiServer.Common.Queries;

namespace Geta.Community.EntityAttributeBuilder
{
    public class MetadataFrameworkEntityQuery<T>
    {
        public FrameworkEntityQueryBase Query { get; private set; }

        public MetadataFrameworkEntityQuery(FrameworkEntityQueryBase query)
        {
            Query = query;
        }

        public ICriterion this[Expression<Func<T, object>> epxr]
        {
            get
            {
                var helper = new ExpressionHelper();
                var propertyName = helper.ExtractMemberAccess(epxr);

                return Query[propertyName];
            }

            set
            {
                var helper = new ExpressionHelper();
                var propertyName = helper.ExtractMemberAccess(epxr);

                Query[propertyName] = value;
            }
        }
    }
}