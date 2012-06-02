using EPiServer.PlugIn;

namespace Geta.ErrorHandler.Configuration
{
    public class Settings
    {
        public static bool IsOldNewUrlRemapperEnabled
        {
            get
            {
                var settings = new AdminUISettingsPlugIn();
                PlugInSettings.AutoPopulate(settings);

                return settings.IsEnabled;
            }
        }
    }
}
