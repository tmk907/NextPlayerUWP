using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Jamendo;
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

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {

            if (Radios.Count == 0)
            {
                var jr = await GetJamendoRadios();
                Radios = new ObservableCollection<RadioItem>(jr);
            }
            await UpdatePlayingNow();

        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            var item = (RadioItem)e.ClickedItem;
            await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            ApplicationSettingsHelper.SaveSongIndex(0);
            PlaybackManager.Current.PlayNew();
        }


        private async Task<List<RadioItem>> GetJamendoRadios()
        {
            List<RadioItem> jamendoRadios = new List<RadioItem>();

            JamendoRadioService jamendoService = new JamendoRadioService();
            jamendoRadios = await jamendoService.GetRadios();

            //JamendoClient client = new JamendoClient();
            //var radiosList = await client.GetRadios();
            //if (radiosList != null)
            //{
            //    foreach (var item in radiosList)
            //    {
            //        RadioItem r = new RadioItem((int)item.Id, NextPlayerUWPDataLayer.Enums.RadioType.Jamendo);
            //        r.Name = item.DisplayName;
            //        r.ImagePath = item.Image;
            //        r.PlayingNowTitle = "";
            //        r.PlayingNowAlbum = "";
            //        r.PlayingNowArtist = "";
            //        r.RemainingTime = 0;
            //        r.StreamUrl = "";
            //        jamendoRadios.Add(r);
            //    }
            //}
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

                    //radio.PlayingNowTitle = streamRadio.Title;
                    //radio.PlayingNowAlbum = streamRadio.Album;
                    //radio.PlayingNowArtist = streamRadio.Artist;
                    //radio.PlayingNowImagePath = streamRadio.CoverUri;

                    //radio.RemainingTime = streamRadio.RemainingSeconds;
                    //radio.StreamUpdatedAt = DateTime.Now;
                }
            }


            //JamendoClient client = new JamendoClient();
            //foreach (var radio in Radios)
            //{
            //    if (radio.RemainingTime < (DateTime.Now - radio.StreamUpdatedAt).TotalMilliseconds)
            //    {
            //        var streamRadio = await client.GetStream(radio.BroadcastId);
            //        if (streamRadio != null)
            //        {
            //            try
            //            {
            //                radio.StreamUrl = streamRadio.StreamUrl;
            //            }
            //            catch (Exception ex)
            //            {

            //            }
            //            radio.PlayingNowTitle = streamRadio.PlayingNow.TrackName;
            //            radio.PlayingNowAlbum = streamRadio.PlayingNow.AlbumName;
            //            radio.PlayingNowArtist = streamRadio.PlayingNow.ArtistName;
            //            radio.PlayingNowImagePath = streamRadio.PlayingNow.TrackImage;

            //            radio.RemainingTime = streamRadio.CallMeBack;
            //            radio.StreamUpdatedAt = DateTime.Now;
            //        }
            //        else
            //        {
            //            try
            //            {
            //                radio.PlayingNowTitle = "";
            //                radio.PlayingNowAlbum = "";
            //                radio.PlayingNowArtist = "";
            //                radio.PlayingNowImagePath = AppConstants.AlbumCoverSmall;
            //            }
            //            catch (Exception)
            //            {

            //                throw;
            //            }
            //        }
            //    }
            //}
        }

        public async void PlayNow(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            ApplicationSettingsHelper.SaveSongIndex(0);
            PlaybackManager.Current.PlayNew();
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
    }
}
