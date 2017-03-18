using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels.Settings;
using System.Collections.Generic;

namespace NextPlayerUWP.Controls
{
    public class SettingsMenuItem
    {
        public string Icon { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public ISettingsViewModel ViewModel { get; set; }

        public static List<SettingsMenuItem> GetMainItems(SettingsVMService service)
        {
            TranslationHelper tr = new TranslationHelper();

            var items = new List<SettingsMenuItem>();
            items.Add(new SettingsMenuItem() { Icon = "\xE8F1", Name = tr.GetTranslation("TBLibrary/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsLibraryViewModel)), TypeName = nameof(SettingsLibraryViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE771", Name = tr.GetTranslation("TBPersonalize/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsPersonalizationViewModel)), TypeName = nameof(SettingsPersonalizationViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE90F", Name = tr.GetTranslation("TBTools/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsToolsViewModel)), TypeName = nameof(SettingsToolsViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE716", Name = tr.GetTranslation("TBAccounts/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsAccountsViewModel)), TypeName = nameof(SettingsAccountsViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE946", Name = tr.GetTranslation("TBAbout/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsAboutViewModel)), TypeName = nameof(SettingsAboutViewModel) });
            return items;
        }

        public static List<SettingsMenuItem> GetOptionsItems()
        {
            var items = new List<SettingsMenuItem>();
            return items;
        }
    }
}
