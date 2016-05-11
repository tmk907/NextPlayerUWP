﻿using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
            JamendoApi.JamendoApiClient client = new JamendoApi.JamendoApiClient(AppConstants.JamendoClientId);
            JamendoApi.ApiCalls.Radios.RadiosCall getRadios = new JamendoApi.ApiCalls.Radios.RadiosCall();
            getRadios.ImageSize = new JamendoApi.ApiCalls.Parameters.ImageSizeParameter<JamendoApi.ApiCalls.Parameters.RadioImageSize>(JamendoApi.ApiCalls.Parameters.RadioImageSize.Px150);
            var radiosList = await client.CallAsync(getRadios);
            if (radiosList.Headers.Status == JamendoApi.ApiEntities.Headers.ResponseStatus.Success)
            {
                foreach (var item in radiosList.Results)
                {
                    RadioItem r = new RadioItem((int)item.Id, NextPlayerUWPDataLayer.Enums.RadioType.Jamendo, "");
                    r.Name = item.DisplayName;
                    r.ImagePath = item.Image;
                    r.PlayingNowTitle = "PlayingNow.Name";
                    r.PlayingNowAlbum = "PlayingNow.AlbumName";
                    r.PlayingNowArtist = "PlayingNow.ArtistName";
                    jamendoRadios.Add(r);
                }
            }
            return jamendoRadios;
        }

        private async Task UpdatePlayingNow()
        {
            JamendoApi.JamendoApiClient client = new JamendoApi.JamendoApiClient(AppConstants.JamendoClientId);
            foreach(var radio in Radios)
            {
                JamendoApi.ApiCalls.Radios.RadioStreamCall radiostream = new JamendoApi.ApiCalls.Radios.RadioStreamCall();
                radiostream.Id = new JamendoApi.ApiCalls.Parameters.IdParameter((uint)radio.BroadcastId);
                var streamRadio = await client.CallAsync(radiostream);
                if (streamRadio.Headers.Status == JamendoApi.ApiEntities.Headers.ResponseStatus.Success)
                {
                    var stream = streamRadio.Results.FirstOrDefault();
                    try
                    {
                        radio.StreamUrl = stream.Stream;
                    }
                    catch (Exception ex)
                    {

                    }
                    radio.PlayingNowTitle = stream.PlayingNow.Name;
                    radio.PlayingNowAlbum = stream.PlayingNow.AlbumName;
                    radio.PlayingNowArtist = stream.PlayingNow.ArtistName;
                    radio.PlayingNowImagePath = stream.PlayingNow.Image;
                }
                else
                {
                    try
                    {
                        radio.PlayingNowTitle = "";
                        radio.PlayingNowAlbum = "";
                        radio.PlayingNowArtist = "";
                        radio.PlayingNowImagePath = AppConstants.AlbumCoverSmall;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
        }
    }
}
