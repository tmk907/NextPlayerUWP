using NextPlayerUWP.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Controls
{
    public class SettingsItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AboutTemplate { get; set; }
        public DataTemplate AccountsTemplate { get; set; }
        public DataTemplate ExtensionsTemplate { get; set; }
        public DataTemplate LibraryTemplate { get; set; }
        public DataTemplate ToolsTemplate { get; set; }
        public DataTemplate PersonalizationTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null && item is ISettingsViewModel)
            {
                if (item.GetType() == typeof(SettingsLibraryViewModel))
                    return LibraryTemplate;
                if (item.GetType() == typeof(SettingsToolsViewModel))
                    return ToolsTemplate;
                if (item.GetType() == typeof(SettingsPersonalizationViewModel))
                    return PersonalizationTemplate;
                if (item.GetType() == typeof(SettingsExtensionsViewModel))
                    return ExtensionsTemplate;
                if (item.GetType() == typeof(SettingsAccountsViewModel))
                    return AccountsTemplate;
                if (item.GetType() == typeof(SettingsAboutViewModel))
                    return AboutTemplate;
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
