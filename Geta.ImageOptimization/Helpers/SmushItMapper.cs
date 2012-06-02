using System.Net;
using Geta.ImageOptimization.Messaging;

namespace Geta.ImageOptimization.Helpers
{
    public static class SmushItMapper
    {
         public static ImageOptimizationResponse ConvertToResponse(this SmushItResponse smushItResponse)
         {
             if (smushItResponse == null)
             {
                 return new ImageOptimizationResponse();
             }

             var webClient = new WebClient();

             var imageOptimizationResponse = new ImageOptimizationResponse
                                                 {
                                                     OriginalImageUrl = smushItResponse.Src,
                                                     OriginalImageSize = smushItResponse.Src_Size,
                                                     PercentSaved = smushItResponse.Percent,
                                                     OptimizedImageSize = smushItResponse.Dest_Size,
                                                     ErrorMessage = smushItResponse.Error
                                                 };

             if (!string.IsNullOrEmpty(smushItResponse.Dest))
             {
                 imageOptimizationResponse.OptimizedImage = webClient.DownloadData(smushItResponse.Dest);
                 imageOptimizationResponse.Successful = true;
             }

             return imageOptimizationResponse;
         }
    }
}