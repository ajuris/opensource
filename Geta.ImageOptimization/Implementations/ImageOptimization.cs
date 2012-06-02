using System.Web;
using Geta.ImageOptimization.Helpers;
using Geta.ImageOptimization.Interfaces;
using Geta.ImageOptimization.Messaging;

namespace Geta.ImageOptimization.Implementations
{
    public class ImageOptimization : IImageOptimization
    {
        private readonly ISmushItProxy _smushItProxy;

        public ImageOptimization() : this(new SmushItProxy())
        {
        }

        public ImageOptimization(ISmushItProxy smushItProxy)
        {
            this._smushItProxy = smushItProxy;
        }

        public ImageOptimizationResponse ProcessImage(ImageOptimizationRequest imageOptimizationRequest)
        {
            if (imageOptimizationRequest == null)
            {
                return new ImageOptimizationResponse();
            }

            var smushItRequest = new SmushItRequest
                                     {
                                         ImageUrl = imageOptimizationRequest.ImageUrl
                                     };
            SmushItResponse smushItResponse = this._smushItProxy.ProcessImage(smushItRequest);

            return smushItResponse.ConvertToResponse();
        }
    }
}