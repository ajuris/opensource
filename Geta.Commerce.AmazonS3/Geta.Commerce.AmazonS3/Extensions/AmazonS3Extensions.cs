using System;
using Amazon.S3.Model;
using Geta.Commerce.AmazonS3.Model;

namespace Geta.Commerce.AmazonS3.Extensions
{
    public static class AmazonS3Extensions
    {
        public static AmazonS3Object ToAmazonS3Object(this GetObjectResponse response)
        {
            var amazonS3Object = new AmazonS3Object();
            amazonS3Object.Name = response.Key;
            amazonS3Object.Id = response.AmazonId2;
            amazonS3Object.Length = response.ContentLength;
            return amazonS3Object;
        }

        public static AmazonS3Object ToAmazonS3Object(this S3Object response)
        {
            var amazonS3Object = new AmazonS3Object();
            amazonS3Object.Name = response.Key;
            amazonS3Object.Modified = Convert.ToDateTime(response.LastModified);
            amazonS3Object.Length = response.Size;
            return amazonS3Object;
        }

        public static string RemovePath(this string path, string removePath)
        {
            return EPiServer.Url.Decode(path).Replace(removePath, string.Empty);
        }
    }
}