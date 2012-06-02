using EPiServer.Web.Hosting;

namespace Geta.AmazonS3.Hosting
{
    public class AmazonS3Summary : UnifiedSummary
    {
        public override bool CanPersist
        {
            get { return true; }
        }

        public override void SaveChanges()
        {
        }
    }
}
