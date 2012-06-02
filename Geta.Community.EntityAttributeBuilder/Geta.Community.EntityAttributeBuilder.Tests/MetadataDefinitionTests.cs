using System;
using System.Collections.Generic;
using EPiServer.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geta.Community.EntityAttributeBuilder.Tests
{
    public class SampleType : FrameworkEntityBase
    {
        public SampleType(int id) : base(id)
        {
        }

        public SampleType(int id, IFrameworkEntity entity) : base(id, entity)
        {
        }

        public override string[] CacheKey
        {
            get { throw new NotImplementedException(); }
        }
    }

    [CommunityEntity]
    public class SampleAttributeMetadata
    {
        [CommunityEntityMetadata]
        public virtual IList<SampleType> SampleAttribute { get; set; }
    }

    [TestClass]
    public class MetadataDefinitionTests
    {
        [TestMethod]
        public void MetadataDefinitionTest_WithAttributeType()
        {
            var metadata = new CommunityEntityMetadataDefinition(new CommunityEntityDefinition
                                                                     {
                                                                         EntityAttribute = new CommunityEntity
                                                                                               {
                                                                                                   TargetType = typeof (SampleType)
                                                                                               }
                                                                     },
                                                                 new CommunityEntityMetadata
                                                                     {
                                                                         Name = "SampleAttribute"
                                                                     },
                                                                 typeof (SampleAttributeMetadata).GetProperty("SampleAttribute"));

            Assert.AreEqual(typeof (SampleType), metadata.AttributeType);
        }

        [TestMethod]
        public void MetadataDefinitionTest_WithClrAttribute()
        {
            var metadata = new CommunityEntityMetadataDefinition(new CommunityEntityDefinition
                                                                     {
                                                                         EntityAttribute = new CommunityEntity
                                                                                               {
                                                                                                   TargetType = typeof (SampleType)
                                                                                               }
                                                                     },
                                                                 new CommunityEntityMetadata
                                                                     {
                                                                         Type = typeof (ICollection<SampleType>),
                                                                         Name = "SampleAttribute"
                                                                     },
                                                                 typeof (SampleAttributeMetadata).GetProperty("SampleAttribute"));

            Assert.AreEqual(typeof (SampleType), metadata.AttributeType);
        }

        [TestMethod]
        public void CollectionInterceptor_GettSetter_Test()
        {
            var entity = new SampleType(-1).AsAttributeExtendable<SampleAttributeMetadata>();

            try
            {
                // getter
                var collection = entity.SampleAttribute;
                collection.Add(new SampleType(100));
                entity.SampleAttribute = collection;
            }
            catch (Exception)
            {
            }


            try
            {
                // setter
                entity.SampleAttribute = new List<SampleType>();
            }
            catch (Exception)
            {
            }
        }


        [TestMethod]
        public void CollectionInterceptor_InlineMethodCall_Test()
        {
            var entity = new SampleType(-1).AsAttributeExtendable<SampleAttributeMetadata>();

            try
            {
                entity.SampleAttribute.Add(new SampleType(100));
            }
            catch (Exception)
            {
            }
        }
    }
}
