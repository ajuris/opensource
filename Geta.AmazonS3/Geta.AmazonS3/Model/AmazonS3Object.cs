using System;

namespace Geta.AmazonS3.Model
{
    public class AmazonS3Object
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public long Length { get; set; }

        public string Url { get; set; }

        public string PermanentLinkUrl { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }
    }
}
