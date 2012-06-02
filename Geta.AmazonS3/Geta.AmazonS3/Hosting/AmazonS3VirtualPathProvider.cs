using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using EPiServer.Web.Hosting;
using Geta.AmazonS3.Repository;
using log4net;

namespace Geta.AmazonS3.Hosting
{
    public class AmazonS3VirtualPathProvider : VirtualPathUnifiedProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AmazonS3VirtualPathProvider(string name, NameValueCollection configParameters) : base(name, configParameters)
        {
            this.BucketName = configParameters["bucketName"];
            const string logMessage = "AmazonS3VirtualPathProvider configuration missing or empty attribute '{0}'";
            if (string.IsNullOrEmpty(this.BucketName))
            {
                Log.Error(string.Format(logMessage, "bucketName"));
                throw new ConfigurationErrorsException(string.Format(logMessage, "bucketName"));
            }

            this.HostName = configParameters["hostName"];
            if (string.IsNullOrEmpty(this.HostName))
            {
                Log.Error(string.Format(logMessage, "hostName"));
                throw new ConfigurationErrorsException(string.Format(logMessage, "hostName"));
            }

            this.AwsAccessKey = configParameters["awsAccessKey"];
            if (string.IsNullOrEmpty(this.AwsAccessKey))
            {
                Log.Error(string.Format(logMessage, "awsAccessKey"));
                throw new ConfigurationErrorsException(string.Format(logMessage, "awsAccessKey"));
            }

            this.AwsSecretKey = configParameters["awsSecretKey"];
            if (string.IsNullOrEmpty(this.AwsSecretKey))
            {
                Log.Error(string.Format(logMessage, "awsSecretKey"));
                throw new ConfigurationErrorsException(string.Format(logMessage, "awsSecretKey"));
            }

            this.Scheme = configParameters["scheme"] ?? "http://";

            this.VirtualPathRoot = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAbsolute(configParameters["virtualPath"]));

            this.ValidateAndSetupConfigParams();
        }

        public string BucketName { get; set; }

        public string AwsAccessKey { get; set; }

        public string AwsSecretKey { get; set; }

        public string HostName { get; set; }

        public string Scheme { get; set; }
       
        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            if (virtualDir.StartsWith(VirtualPathRoot))
            {
                var directory = new AmazonS3Directory(this, virtualDir);
                return directory;
            }

            return Previous.GetDirectory(virtualDir);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            if (virtualPath.StartsWith(this.VirtualPathRoot))
            {
                var amazonFile = new AmazonS3Repository(this.AwsAccessKey, this.AwsSecretKey, this.BucketName).GetFile(EPiServer.Url.Decode(virtualPath).Replace(VirtualPathRoot, string.Empty));
                var parent = new AmazonS3Directory(this, VirtualPathUtility.GetDirectory(virtualPath));
                
                if (amazonFile == null)
                {
                    return null;
                }

                return new AmazonS3File(parent, this, virtualPath, true, amazonFile);
            }

            if (string.IsNullOrEmpty(virtualPath))
            {
                return null;
            }

            return this.Previous.GetFile(virtualPath);
        }
    }
}