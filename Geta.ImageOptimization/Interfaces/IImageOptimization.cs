using Geta.ImageOptimization.Messaging;

namespace Geta.ImageOptimization.Interfaces
{
    public interface IImageOptimization
    {
        ImageOptimizationResponse ProcessImage(ImageOptimizationRequest imageOptimizationRequest);
    }
}