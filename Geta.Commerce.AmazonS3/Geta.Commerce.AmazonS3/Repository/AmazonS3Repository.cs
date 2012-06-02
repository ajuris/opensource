using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon;
using Amazon.S3.Model;
using Geta.Commerce.AmazonS3.Extensions;
using Geta.Commerce.AmazonS3.Model;
using log4net;

namespace Geta.Commerce.AmazonS3.Repository
{
    public class AmazonS3Repository : IAmazonS3Repository
    {
        private readonly string _bucketName;
        private readonly Amazon.S3.AmazonS3 _client;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public AmazonS3Repository(string accessKey, string secretKey, string bucketName)
        {
            this._client = AWSClientFactory.CreateAmazonS3Client(accessKey, secretKey);
            this._bucketName = bucketName;
        }

        public IEnumerable<S3Object> GetFiles(string folder)
        {
            var request = new ListObjectsRequest().WithBucketName(this._bucketName).WithPrefix(folder).WithDelimiter("/");

            using (var response = this._client.ListObjects(request))
            {
                return response.S3Objects.Where(o => o.Key.Last() != '/');
            }
        }

        public IEnumerable<string> GetFolder(string folder)
        {
            var request = new ListObjectsRequest().WithBucketName(this._bucketName).WithPrefix(folder);
            using (var response = this._client.ListObjects(request))
            {
                if (folder == string.Empty || folder == "/")
                {
                    // get the objects at the TOP LEVEL, i.e. not inside any folders
                    var objects = response.S3Objects.Where(o => !o.Key.Contains(@"/"));

                    // get the folders at the TOP LEVEL only
                    return response.S3Objects.Except(objects).Where(o => o.Key.Last() == '/' && o.Key.IndexOf(@"/") == o.Key.LastIndexOf(@"/")).Select(n => n.Key);
                }


                var directories = new List<string>();

                foreach (var split in response.S3Objects.Select(s3Object => s3Object.Key.Replace(folder, string.Empty).Split('/')).Where(splits => splits.Count() > 1 && !directories.Contains(splits.First())))
                {
                    directories.Add(split.First());
                }

                return directories;
            }
        }

        public void CreateFolder(string bucket, string folder)
        {
            var key = string.Format(@"{0}/", folder);

            if (this._client.ListObjects(new ListObjectsRequest().WithBucketName(this._bucketName).WithPrefix(folder)).S3Objects.Count > 0)
            {
                return;
            }

            var request = new PutObjectRequest().WithBucketName(bucket).WithKey(key).WithCannedACL(S3CannedACL.PublicRead);
            request.InputStream = new MemoryStream();
            this._client.PutObject(request);
        }

        public AmazonS3Object GetFile(string key)
        {
            var request = new GetObjectRequest();
            request.WithBucketName(this._bucketName).WithKey(key);

            try
            {
                var response = this._client.GetObject(request);
                return response.ToAmazonS3Object();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Cannot find the file with key {0}. Debug message: {1}", key, ex.Message));
                return null;
            }
        }

        public void SetFileToPublic(string key)
        {
            try
            {
                var request = new SetACLRequest();
                request.WithBucketName(this._bucketName).WithKey(key);
                request.CannedACL = S3CannedACL.PublicRead;
                this._client.SetACL(request);
            }
            catch
            {
                // File not found
            }
        }

        public void Delete(string key)
        {
            var deleteObject = new DeleteObjectRequest();
            deleteObject.WithBucketName(this._bucketName).WithKey(key);
            this._client.DeleteObject(deleteObject);
        }

        public void Upload(string fileName, Stream stream)
        {
            var request = new PutObjectRequest();
            request.WithBucketName(this._bucketName).WithKey(fileName).WithInputStream(stream);
            request.CannedACL = S3CannedACL.PublicRead;
            request.StorageClass = S3StorageClass.ReducedRedundancy;
            S3Response response = this._client.PutObject(request);
            response.Dispose();
        }

        public void Copy(string copyFrom, string copyTo)
        {
            try
            {
                var copyRequest = new CopyObjectRequest().WithSourceBucket(this._bucketName).WithDestinationBucket(this._bucketName).WithSourceKey(copyFrom).WithDestinationKey(copyTo).WithCannedACL(S3CannedACL.PublicReadWrite);
                this._client.CopyObject(copyRequest);               
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Cannot copy file from {0} to {1}. Debug message: {2}", copyFrom, copyTo, ex.Message));
                return;
            }
        }

        public void Move(string moveFrom, string moveTo)
        {
            try
            {
                var copyRequest = new CopyObjectRequest().WithSourceBucket(this._bucketName).WithDestinationBucket(this._bucketName).WithSourceKey(moveFrom).WithDestinationKey(moveTo).WithCannedACL(S3CannedACL.PublicReadWrite);
                this._client.CopyObject(copyRequest);
                this.Delete(moveFrom);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Cannot move file from {0} to {1}. Debug message: {2}", moveFrom, moveTo, ex.Message));
                return;
            }
        }
    }
}