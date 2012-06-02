using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using EPiServer.Security;
using EPiServer.Web.Hosting;
using Geta.Commerce.AmazonS3.Extensions;
using Geta.Commerce.AmazonS3.Repository;

namespace Geta.Commerce.AmazonS3.Hosting
{
    public class AmazonS3Directory : UnifiedDirectory
    {
        private readonly AmazonS3Repository _amazon;

        public AmazonS3Directory(VirtualPathUnifiedProvider provider, string virtualPath) : base(provider, virtualPath, null, true)
        {
            this._amazon = new AmazonS3Repository(((AmazonS3VirtualPathProvider)this.Provider).AwsAccessKey, ((AmazonS3VirtualPathProvider)this.Provider).AwsSecretKey, ((AmazonS3VirtualPathProvider)this.Provider).BucketName);
        }

        public override IEnumerable Files
        {
            get 
            {
                return this._amazon.GetFiles(EPiServer.Url.Decode(this.VirtualPath).Replace(this.Provider.VirtualPathRoot, string.Empty)).Select(amazonFile => new AmazonS3File(this, this.Provider, this.Provider.VirtualPathRoot + amazonFile.Key, this.Provider.BypassAccessCheck, amazonFile.ToAmazonS3Object()));
            }
        }

        public override AccessControlList ACL
        {
            get
            {
                var accessControlList = new AccessControlList();

                if (this.Provider.BypassAccessCheck)
                {
                    accessControlList.Add(new AccessControlEntry("Everyone", AccessLevel.FullAccess, SecurityEntityType.Role));
                }

                return accessControlList;
            }
        }

        public override IEnumerable Directories
        {
            get
            {
                return this._amazon.GetFolder(EPiServer.Url.Decode(this.VirtualPath).Replace(this.Provider.VirtualPathRoot, string.Empty)).Select(amazonFolder => new AmazonS3Directory(this.Provider, this.Provider.VirtualPathRoot + amazonFolder));
            }
        }

        public override IEnumerable Children
        {
            get
            {
                yield return this.Files;
                yield return this.Directories;
            }
        }

        public override bool IsFirstLevel
        {
            get
            {
                return this.Parent == null;
            }
        }

        public override UnifiedDirectory Parent
        {
            get
            {
                return HostingEnvironment.VirtualPathProvider.GetDirectory(VirtualPathUtility.GetDirectory(VirtualPath)) as AmazonS3Directory;
            }
        }

        public override UnifiedDirectory[] GetDirectories()
        {
            return this.Directories.Cast<AmazonS3Directory>().ToArray();
        }

        public override UnifiedFile[] GetFiles()
        {
            return this.Files.Cast<AmazonS3File>().ToArray();
        }

        public override UnifiedDirectory CreateSubdirectory(string path)
        {
            this._amazon.CreateFolder(((AmazonS3VirtualPathProvider)this.Provider).BucketName, EPiServer.Url.Decode(this.VirtualPath).Replace(this.Provider.VirtualPathRoot, string.Empty) + path);
            return new AmazonS3Directory(this.Provider, this.VirtualPath + path);
        }

        public override void Delete()
        {
            this._amazon.Delete(EPiServer.Url.Decode(this.VirtualPath).Replace(this.Provider.VirtualPathRoot, string.Empty));
        }

        public override UnifiedFile CreateFile(string name)
        {
            return CreateFile(name, Guid.NewGuid());
        }

        public override UnifiedFile CreateFile(string name, Guid id)
        {
            var virtualPath = Provider.CombineVirtualPaths(VirtualPath, name);
            return new AmazonS3File(this, this.Provider, virtualPath, this.Provider.BypassAccessCheck, null);
        }
    }
}
