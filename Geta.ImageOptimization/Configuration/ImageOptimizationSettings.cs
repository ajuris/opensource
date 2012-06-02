using System.Configuration;

namespace Geta.ImageOptimization.Configuration
{
    public class ImageOptimizationSettings : ConfigurationSection
    {
        private static ImageOptimizationSettings settings = ConfigurationManager.GetSection("ImageOptimizationSettings") as ImageOptimizationSettings;

        public static ImageOptimizationSettings Settings { get { return settings; } }

        /// <summary>
        /// Url prefix used for the images (needs to be public)
        /// </summary>
        [ConfigurationProperty("siteUrl")]
        public string SiteUrl
        {
            get { return (string)this["siteUrl"]; }
            set { this["siteUrl"] = value; }
        }

        /// <summary>
        /// Separated by comma: Page Files,Global Files,Documents
        /// </summary>
        [ConfigurationProperty("virtualNames")]
        public string VirtualNames
        {
            get { return (string)this["virtualNames"]; }
            set { this["virtualNames"] = value; }
        }
    }
}