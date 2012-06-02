using System.Collections.Generic;
using System.IO;
using Amazon.S3.Model;
using Geta.Commerce.AmazonS3.Model;

namespace Geta.Commerce.AmazonS3.Repository
{
    public interface IAmazonS3Repository
    {
        IEnumerable<S3Object> GetFiles(string folder);

        IEnumerable<string> GetFolder(string folder);

        void CreateFolder(string bucket, string folder);

        AmazonS3Object GetFile(string key);

        void Delete(string key);

        void Upload(string fileName, Stream stream);

        void Copy(string copyFrom, string copyTo);

        void Move(string moveFrom, string moveTo);

        void SetFileToPublic(string key);
    }
}