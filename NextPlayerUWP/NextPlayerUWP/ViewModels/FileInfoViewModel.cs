using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class FileInfoViewModel : Template10.Mvvm.ViewModelBase
    {
        private SongData fileInfo = new SongData();
        public SongData FileInfo
        {
            get { return fileInfo; }
            set { Set(ref fileInfo, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            fileInfo = new SongData();
            if (parameter != null)
            {
                int songId = Int32.Parse(MusicItem.ParseParameter(parameter as string)[1]);
                FileInfo = await DatabaseManager.Current.GetSongDataAsync(songId);
                await AddFileSize();
            }
        }

        private async Task AddFileSize()
        {
            try
            {
                Windows.Storage.IStorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(fileInfo.Path);
                FileInfo.FileSize = file.OpenAsync(Windows.Storage.FileAccessMode.Read).AsTask().Result.Size;
            }
            catch (Exception ex)
            {
                //App.TelemetryClient.TrackTrace("AddFileSize" + Environment.NewLine + ex.Message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
            }
        }
    }
}
