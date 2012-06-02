namespace Geta.ImageOptimization.Messaging
{
    public class ImageOptimizationResponse
    {
        public string OriginalImageUrl { get; set; }

        public int OriginalImageSize { get; set; }

        public decimal PercentSaved { get; set; }

        public byte[] OptimizedImage { get; set; }

        public int OptimizedImageSize { get; set; }

        public bool Successful { get; set; }

        public string ErrorMessage { get; set; }
    }
}