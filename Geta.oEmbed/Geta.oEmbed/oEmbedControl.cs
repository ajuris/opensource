using System.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;
using EPiServer;

namespace Geta.oEmbed
{
    public class oEmbedControl : Control
    {
        public oEmbedOptions Options { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Options == null || string.IsNullOrEmpty(Options.Url))
            {
                return;
            }

            string jsonResponse = string.Empty;
            string endpoint = this.BuildUrl();

            var webClient = new System.Net.WebClient();

            string result;

            try
            {
                jsonResponse = webClient.DownloadString(endpoint);
            }
            catch (System.Net.WebException exception)
            {
                if (exception.Status != System.Net.WebExceptionStatus.ProtocolError)
                {
                    throw;
                }
                // If it's a ProtocolError (404).
                result = "<p><a href=\"" + this.Options.Url + "\">" + this.Options.Url + "</a>. Error with embedding the source</p>";

                writer.Write(result);
            }

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var jSerialize = new JavaScriptSerializer();
                var oEmbedResponse = jSerialize.Deserialize<oEmbedResponse>(jsonResponse);
                result = oEmbedResponse.RenderMarkup();

                writer.Write(result);
            }
        }

        private string BuildUrl()
        {
            var endpoint = "http://api.embed.ly/1/oembed";

            if (this.Options.MaxWidth > 0)
            {
                endpoint = UriSupport.AddQueryString(endpoint, "maxwidth", this.Options.MaxWidth.ToString());
            }
            if (this.Options.MaxHeight > 0)
            {
                endpoint = UriSupport.AddQueryString(endpoint, "maxheight", this.Options.MaxHeight.ToString());
            }

            endpoint = UriSupport.AddQueryString(endpoint, "url", this.Options.Url);

            string key = ConfigurationManager.AppSettings["EmbedKey"];

            if (string.IsNullOrEmpty(key))
            {
                throw new ConfigurationErrorsException("Missing key for Geta.oEmbed. Add it to your sites AppSettings: <add key=\"EmbedKey\" value=\"your-key\" />. You can get your key at: http://embed.ly/");
            }
            
            endpoint = UriSupport.AddQueryString(endpoint, "key", key);

            return endpoint;
        }
    }
}