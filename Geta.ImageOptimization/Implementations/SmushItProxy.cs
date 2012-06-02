using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using EPiServer;
using Geta.ImageOptimization.Interfaces;
using Geta.ImageOptimization.Messaging;

namespace Geta.ImageOptimization.Implementations
{
    public class SmushItProxy : ISmushItProxy
    {
        private readonly WebClient _webClient = new WebClient();
        private readonly JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();

        public SmushItResponse ProcessImage(SmushItRequest smushItRequest)
        {
            string jsonResponse = string.Empty;

            string endpoint = this.BuildUrl(smushItRequest.ImageUrl);

            try
            {
                jsonResponse = this._webClient.DownloadString(endpoint);
            }
            catch (WebException exception)
            {
                throw new WebException(exception.Message);
            }

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                return this._javaScriptSerializer.Deserialize<SmushItResponse>(jsonResponse);
            }

            return new SmushItResponse { Src = smushItRequest.ImageUrl };
        }

        private string BuildUrl(string imageUrl)
        {
            string endpoint = "http://www.smushit.com/ysmush.it/ws.php";

            endpoint = UriSupport.AddQueryString(endpoint, "img", HttpUtility.UrlEncode(imageUrl));

            return endpoint;
        }
    }
}