using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Common;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class NowPlayingPlaylistViewModel : Template10.Mvvm.ViewModelBase
    {
        int firstVisibleIndex = 0;
        string positionKey;
        ListView listView;

        public NowPlayingPlaylistViewModel()
        {
            UpdatePlaylist();
            NowPlayingPlaylistManager.NPListChanged += NPListChanged;
            PlaybackService.MediaPlayerTrackChanged += TrackChanged;
            lastFmCache = new LastFmCache();
        }

        LastFmCache lastFmCache;

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set { Set(ref selectedPivotIndex, value); }
        }

        private void TrackChanged(int index)
        {
            Dispatcher.Dispatch(() =>
            {
                if (songs.Count == 0 || index > songs.Count - 1 || index < 0) return;
                CurrentSong = songs[index];
                if (!CurrentSong.IsAlbumArtSet)
                {

                }
                else
                {
                    CoverUri = CurrentSong.AlbumArtUri;
                }
                ScrollAfterTrackChanged(index);
            });
        }

        private void NPListChanged()
        {
            UpdatePlaylist();
        }

        private ObservableCollection<SongItem> songs;
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        private SongItem currentSong = new SongItem();
        public SongItem CurrentSong
        {
            get { return currentSong; }
            set { Set(ref currentSong, value); }
        }

        private Uri coverUri;
        public Uri CoverUri
        {
            get { return coverUri; }
            set { Set(ref coverUri, value); }
        }

        private void UpdatePlaylist()
        {
            Songs = NowPlayingPlaylistManager.Current.songs;
        }

        public async void OnLoaded(ListView p)
        {
            listView = p;
            firstVisibleIndex = ApplicationSettingsHelper.ReadSongIndex();
            if (firstVisibleIndex >= songs.Count || firstVisibleIndex < 0)
            {
                firstVisibleIndex = 0;
            }
            else
            {
                await SetScrollPosition();
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            //App.ChangeRightPanelVisibility(true);
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;
            positionKey = ListViewPersistenceHelper.GetRelativeScrollPosition(listView, ItemToKeyHandler);
            var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
            firstVisibleIndex = isp.FirstVisibleIndex;

            if (suspending)
            {
                pageState[nameof(firstVisibleIndex)] = firstVisibleIndex;
                pageState[nameof(positionKey)] = positionKey;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            //App.ChangeRightPanelVisibility(false);
            CoverUri = SongCoverManager.Instance.GetCurrent();
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            int i = ApplicationSettingsHelper.ReadSongIndex();
            if (i < songs.Count && i >= 0)
            {
                CurrentSong = songs[i];
            }
            if (state.ContainsKey(nameof(firstVisibleIndex)))
            {
                firstVisibleIndex = (int)state[nameof(firstVisibleIndex)];
            }
            if (state.ContainsKey(nameof(positionKey)))
            {
                positionKey = (string)state[nameof(positionKey)];
            }

            await Task.CompletedTask;
        }

        private void ScrollAfterTrackChanged(int index)
        {
            var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
            int firstVisibleIndex = isp.FirstVisibleIndex;
            int lastVisibleIndex = isp.LastVisibleIndex;
            if (index <= lastVisibleIndex && index >= firstVisibleIndex) return;
            if (index < firstVisibleIndex)
            {
                listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
            }
            else if (index > lastVisibleIndex)
            {
                listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Default);
            }
        }

        private async Task SetScrollPosition()
        {
            listView.ScrollIntoView(listView.Items[firstVisibleIndex], ScrollIntoViewAlignment.Leading);
            if (positionKey != null)
            {
                await ListViewPersistenceHelper.SetRelativeScrollPositionAsync(listView, positionKey, KeyToItemHandler);
            }
        }

        private string ItemToKeyHandler(object item)
        {
            if (item == null) return null;
            MusicItem mi = (MusicItem)item;
            return mi.GetParameter();
        }

        private IAsyncOperation<object> KeyToItemHandler(string key)
        {
            return Task.Run(() =>
            {
                if (listView.Items.Count <= 0)
                {
                    return null;
                }
                else
                {
                    var i = listView.Items[firstVisibleIndex];
                    if (((MusicItem)i).GetParameter() == key)
                    {
                        return i;
                    }
                    foreach (var item in listView.Items)
                    {
                        if (((MusicItem)item).GetParameter() == key) return item;
                    }
                    return null;
                }
            }).AsAsyncOperation();
        }

        #region Commands

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            foreach (var s in songs)
            {
                if (s.SongId == ((SongItem)e.ClickedItem).SongId) break;
                index++;
            }
            await PlaybackService.Instance.PlayNewList(index);
        }

        public async void Delete(object sender, RoutedEventArgs e)
        {
            if (songs.Count == 1) return;
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.Delete(item.SongId);
        }

        public void Share(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            // App.Current.NavigationService.Navigate(App.Pages.BluetoothSharePage, item.GetParameter()); TODO
        }

        public async void Pin(object sender, RoutedEventArgs e)
        {
            await TileManager.CreateTile((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
        }

        public void EditTags(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            App.Current.NavigationService.Navigate(App.Pages.TagsEditor, item.GetParameter());
        }

        public void ShowDetails(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            App.Current.NavigationService.Navigate(App.Pages.FileInfo, item.GetParameter());
        }

        public void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            App.Current.NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }

        public async void GoToArtist(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            ArtistItem temp = await DatabaseManager.Current.GetArtistItemAsync(item.FirstArtist);
            App.Current.NavigationService.Navigate(App.Pages.Artist, temp.ArtistId);
        }

        public async void GoToAlbum(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            AlbumItem temp = await DatabaseManager.Current.GetAlbumItemAsync(item.Album, item.AlbumArtist);
            App.Current.NavigationService.Navigate(App.Pages.Album, temp.AlbumId);
        }

        #endregion

        public async void RateSong(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (currentSong.SourceType == MusicSource.LocalFile || currentSong.SourceType == MusicSource.LocalNotMusicLibrary)
            {
                int rating = Int32.Parse(button.Tag.ToString());
                currentSong.Rating = rating;
                await lastFmCache.RateSong(currentSong.Artist, currentSong.Title, rating);
                await DatabaseManager.Current.UpdateRatingAsync(currentSong.SongId, currentSong.Rating).ConfigureAwait(false);
            }
        }

        public void SavePlaylist()
        {
            NowPlayingListItem item = new NowPlayingListItem();
            NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }

        public void ChangeCoverUri(Uri cacheUri)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                CoverUri = cacheUri;
            });
        }
    }
}
