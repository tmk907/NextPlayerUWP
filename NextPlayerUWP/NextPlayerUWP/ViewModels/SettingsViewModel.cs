using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class SettingsViewModel : Template10.Mvvm.ViewModelBase
    {
        private string updateProgress = "";
        public string UpdateProgress
        {
            get { return updateProgress; }
            set { Set(ref updateProgress, value); }
        }

        public async void UpdateLibrary()
        {
            MediaImport m = new MediaImport();
            Progress<int> progress = new Progress<int>(
            percent =>
            {
                UpdateProgress = percent.ToString();
            });
            await Task.Run(() => m.UpdateDatabase(progress));
        }
    }
}
