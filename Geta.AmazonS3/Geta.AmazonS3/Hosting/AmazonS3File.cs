using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using EPiServer.Security;
using EPiServer.Web.Hosting;
using Geta.AmazonS3.Extensions;
using Geta.AmazonS3.Model;
using Geta.AmazonS3.Repository;
using log4net;

namespace Geta.AmazonS3.Hosting
{
    public class AmazonS3File : UnifiedFile
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AmazonS3Repository _amazon;
        private readonly AmazonS3Object _file;
        private readonly AmazonS3Summary _summary;
        private readonly string _virtualPath;

        public AmazonS3File(UnifiedDirectory directory,
                            VirtualPathUnifiedProvider provider,
                            string virtualPath,
                            bool bypassAccessCheck,
                            AmazonS3Object file) : base(directory, provider, virtualPath, bypassAccessCheck)
        {
            this._amazon = new AmazonS3Repository(((AmazonS3VirtualPathProvider)Provider).AwsAccessKey, 
                                                  ((AmazonS3VirtualPathProvider)Provider).AwsSecretKey, 
                                                  ((AmazonS3VirtualPathProvider)Provider).BucketName);
            
            this._summary = new AmazonS3Summary();
            this._virtualPath = virtualPath;

            if (file != null)
            {
                this._file = file;
            }
        }

        public string AmazonKey
        {
            get
            {
                return EPiServer.Url.Decode(VirtualPath).Replace(Provider.VirtualPathRoot, string.Empty);
            }
        }

        public override DateTime Changed
        {
            get
            {
                return Convert.ToDateTime(this._file.Modified);
            }
        }

        public override DateTime Created
        {
            get
            {
                return Convert.ToDateTime(this._file.Created);
            }
        }

        public override bool IsDirectory
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return this._file.Length;
            }
        }

        public override string LocalPath
        {
            get
            {
                var provider = (AmazonS3VirtualPathProvider)Provider;

                string baseUrl = VirtualPathUtility.AppendTrailingSlash(provider.Scheme + provider.HostName);

                return string.Format("{0}{1}", baseUrl, Name);
            }
        }

        public override string Name
        {
            get
            {
                return VirtualPathUtility.GetFileName(this._virtualPath);
            }
        }

        public override UnifiedDirectory Parent
        {
            get
            {
                return
                    HostingEnvironment.VirtualPathProvider.GetDirectory(VirtualPathUtility.GetDirectory(VirtualPath)) as AmazonS3Directory;
            }
        }

        public override string PermanentLinkVirtualPath
        {
            get
            {
                return LocalPath;
            }
        }

        public override IUnifiedSummary Summary
        {
            get
            {
                return this._summary;
            }
        }

        public string Url
        {
            get
            {
                return LocalPath;
            }
        }

        public override void CopyTo(string newVirtualPath)
        {
            CopyTo(newVirtualPath, new Guid());
        }

        public override void CopyTo(string newVirtualPath, Guid fileId)
        {
            this._amazon.Copy(VirtualPath.RemovePath(Provider.VirtualPathRoot), newVirtualPath.RemovePath(Provider.VirtualPathRoot));
        }

        public override void Delete()
        {
            this._amazon.Delete(AmazonKey);
        }

        public override void MoveTo(string newVirtualPath)
        {
            var e = new UnifiedVirtualPathEventArgs(newVirtualPath, VirtualPath);

            if (e.Cancel)
            {
                Log.Error(e.Reason);
                throw new OperationAbortedException(e.Reason);
            }

            this._amazon.Move(VirtualPath.RemovePath(Provider.VirtualPathRoot), newVirtualPath.RemovePath(Provider.VirtualPathRoot));
        }

        public override Stream Open()
        {
            return Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public override Stream Open(FileMode mode)
        {
            return Open(mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite);
        }

        public override Stream Open(FileMode mode, FileAccess access)
        {
            return Open(mode, access, FileShare.Read);
        }

        public Stream Open(FileMode mode, FileAccess access, Stream inputStream)
        {
            return OpenInternal(mode, inputStream);
        }

        public override Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return OpenInternal(mode, HttpContext.Current.Request.Files[0].InputStream);
        }

        public Stream OpenRead()
        {
            try
            {
                var client = new WebClient();
                var fileByte = client.DownloadData(LocalPath);
                return new MemoryStream(fileByte);
            }
            catch (Exception ex)
            {
                Log.Error("File {0} not exists");
                return new MemoryStream();
            }
        }

        public override AccessLevel QueryAccess()
        {
            return AccessLevel.Read;
        }

        private Stream OpenInternal(FileMode mode, Stream inputStream)
        {
            if (mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.OpenOrCreate)
            {
                const string amazons3Tempfilename = "AmazonS3TempFileName";
                const string tempExtension = ".TMP";

                var sessionAmazonS3TempFileName = HttpContext.Current.Session[amazons3Tempfilename];
                if (sessionAmazonS3TempFileName != null && !string.IsNullOrEmpty(sessionAmazonS3TempFileName.ToString()))
                {
                    this._amazon.Move(sessionAmazonS3TempFileName.ToString(), VirtualPath.RemovePath(Provider.VirtualPathRoot));
                    HttpContext.Current.Session.Remove(amazons3Tempfilename);
                }
                else
                {
                    this._amazon.Upload(
                                        EPiServer.Url.Decode(VirtualPath).Replace(Provider.VirtualPathRoot, string.Empty), 
                                        inputStream);
                }

                if (VirtualPathUtility.GetExtension(VirtualPath).ToLower() == tempExtension.ToLower())
                {
                    HttpContext.Current.Session.Add(
                                                    amazons3Tempfilename, 
                                                    EPiServer.Url.Decode(VirtualPath).Replace(Provider.VirtualPathRoot, string.Empty));
                }
            }

            return OpenRead();
        }
    }
}
