using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Helpers;
using NextPlayerWinUIDataLayer.Model;
using NextPlayerWinUIDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;


namespace NextPlayerWinUI.ViewModels
{
    public class RadiosViewModel : NextPlayerWinUI.Navigation.ViewModelBase
    {
        private ObservableCollection<RadioItem> favouriteRadios = new ObservableCollection<RadioItem>();
        public ObservableCollection<RadioItem> FavouriteRadios
        {
            get { return favouriteRadios; }
            set { SetProperty(ref favouriteRadios, value); }
        }

        private ObservableCollection<SongItem> streams = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Streams
        {
            get { return streams; }
            set { SetProperty(ref streams, value); }
        }

        private bool updating = false;
        public bool Updating
        {
            get { return updating; }
            set { SetProperty(ref updating, value); }
        }

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set
            {
                if (value == 2)
                {
                    NavigationService.Navigate(AppPages.Pages.CuteRadio);
                }
                else
                {
                    SetProperty(ref selectedPivotIndex, value);
                }
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            App.OnNavigatedToNewView(true);
            Updating = true;
            if (streams.Count == 0)
            {
                var all = await DatabaseManager.Current.GetAllSongItemsAsync();
                Streams = new ObservableCollection<SongItem>(all.Where(s => s.SourceType == NextPlayerWinUIDataLayer.Enums.MusicSource.OnlineFile));
            }
            //if (Radios.Count == 0)
            //{
            //    var jr = await GetJamendoRadios();
            //    Radios = new ObservableCollection<RadioItem>(jr);
            //}
            //await UpdatePlayingNow();
            FavouriteRadios.Clear();
            var list = await DatabaseManager.Current.GetRadioFavouritesAsync();
            foreach(var item in list)
            {
                FavouriteRadios.Add(new RadioItem(item.BroadcastId, item.Type)
                {
                    Name = item.Name,
                    StreamUrl = item.Stream,
                });
            }
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

        public int EditedId = -1;
        private string newName = "";
        public string NewName
        {
            get { return newName; }
            set { SetProperty(ref newName, value); }
        }

        public async void SaveEditedName()
        {
            if (EditedId == -1) return;
            FavouriteRadios.FirstOrDefault(r => r.BroadcastId == EditedId).Name = NewName;
            await DatabaseManager.Current.UpdateFavouriteRadioName(EditedId, NewName);
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
            foreach (var radio in FavouriteRadios)
            {
                if (radio.Type == NextPlayerWinUIDataLayer.Enums.RadioType.Jamendo && radio.RemainingTime < (DateTime.Now - radio.StreamUpdatedAt).TotalMilliseconds)
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
            NavigationService.Navigate(AppPages.Pages.AddToPlaylist, item.GetParameter());
        }

        public async void DeleteFromFavourites(object sender, RoutedEventArgs e)
        {
            var item = (RadioItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await DatabaseManager.Current.DeleteRadioFromFavourites(item.BroadcastId, item.Type);
            FavouriteRadios.Remove(item);
        }
    }
}
