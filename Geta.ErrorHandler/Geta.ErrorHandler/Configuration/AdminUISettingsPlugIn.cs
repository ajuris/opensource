using EPiServer.PlugIn;

namespace Geta.ErrorHandler.Configuration
{
    [GuiPlugIn(Area = PlugInArea.None, DisplayName = "Settings")]
    public class AdminUISettingsPlugIn
    {
        private bool isEnabled;

        public AdminUISettingsPlugIn()
        {
            // by default old URL remapping is enabled
            this.isEnabled = true;
        }

        [PlugInProperty(Description = "Old URL mapping enabled", AdminControl = typeof(DefaultOnCheckBox), AdminControlValue = "Checked")]
        public bool IsEnabled
        {
            get
            {
                return this.isEnabled;
            }

            set
            {
                this.isEnabled = value;
            }
        }
    }
}
