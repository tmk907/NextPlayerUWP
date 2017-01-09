using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace NextPlayerUWP.ViewModels
{
    public class RadiosViewModel : Template10.Mvvm.ViewModelBase
    {
        private ObservableCollection<RadioItem> radios = new ObservableCollection<RadioItem>();
        public ObservableCollection<RadioItem> Radios
        {
            get { return radios; }
            set { Set(ref radios, value); }
        }

        private ObservableCollection<SongItem> streams = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Streams
        {
            get { return streams; }
            set { Set(ref streams, value); }
        }

        private bool updating = false;
        public bool Updating
        {
            get { return updating; }
            set { Set(ref updating, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            App.OnNavigatedToNewView(true);
            Updating = true;
            if (streams.Count == 0)
            {
                var all = await DatabaseManager.Current.GetAllSongItemsAsync();
                Streams = new ObservableCollection<SongItem>(all.Where(s => s.SourceType == NextPlayerUWPDataLayer.Enums.MusicSource.OnlineFile));
            }
            //if (Radios.Count == 0)
            //{
            //    var jr = await GetJamendoRadios();
            //    Radios = new ObservableCollection<RadioItem>(jr);
            //}
            //await UpdatePlayingNow();
            Updating = false;
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            var item = (MusicItem)e.ClickedItem;
            await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            await PlaybackService.Instance.PlayNewList(0);
        }


        private async Task<List<RadioItem>> GetJamendoRadios()
        {
            List<RadioItem> jamendoRadios = new List<RadioItem>();

            JamendoRadioService jamendoService = new JamendoRadioService();
            jamendoRadios = await jamendoService.GetRadios();

            return jamendoRadios;
        }

        private async Task UpdatePlayingNow()
        {
            JamendoRadioService jamendoService = new JamendoRadioService();
            foreach (var radio in Radios)
            {
                if (radio.Type == NextPlayerUWPDataLayer.Enums.RadioType.Jamendo && radio.RemainingTime < (DateTime.Now - radio.StreamUpdatedAt).TotalMilliseconds)
                {
                    var streamRadio = await jamendoService.GetRadioStream(radio.BroadcastId);

                    radio.UpdateStream(streamRadio);
                }
            }
        }

        public async void PlayNow(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async void PlayNext(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.AddNext(item);
        }

        public async void AddToNowPlaying(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.Add(item);
        }

        public void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }
    }
}
