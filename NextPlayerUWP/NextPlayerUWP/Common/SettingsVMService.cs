using NextPlayerUWP.ViewModels.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NextPlayerUWP.Common
{
    public class SettingsVMService
    {
        public List<ISettingsViewModel> viewModels { get; private set; }

        public SettingsVMService()
        {
            viewModels = new List<ISettingsViewModel>();
            viewModels.Add(new SettingsLibraryViewModel());
            viewModels.Add(new SettingsPersonalizationViewModel());
            viewModels.Add(new SettingsToolsViewModel());
            viewModels.Add(new SettingsAccountsViewModel());
            viewModels.Add(new SettingsAboutViewModel());
        }

        public ISettingsViewModel GetViewModelByName(string name)
        {
            switch (name)
            {
                case nameof(SettingsAboutViewModel):
                    return viewModels.OfType<SettingsAboutViewModel>().First();
                case nameof(SettingsAccountsViewModel):
                    return viewModels.OfType<SettingsAccountsViewModel>().First();
                case nameof(SettingsLibraryViewModel):
                    return viewModels.OfType<SettingsLibraryViewModel>().First();
                case nameof(SettingsPersonalizationViewModel):
                    return viewModels.OfType<SettingsPersonalizationViewModel>().First();
                case nameof(SettingsToolsViewModel):
                    return viewModels.OfType<SettingsToolsViewModel>().First();
                default:
                    return null;
            }
        }
    }
}
