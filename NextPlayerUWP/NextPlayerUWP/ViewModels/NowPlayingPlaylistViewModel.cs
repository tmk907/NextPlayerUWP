using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
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
            PlaybackManager.MediaPlayerTrackChanged += TrackChanged;
            Initialize();
        }

        private async Task Initialize()
        {
            int i = ApplicationSettingsHelper.ReadSongIndex();
            CurrentSong = songs[i];
            await ChangeCover();
        }

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set { Set(ref selectedPivotIndex, value); }
        }

        private async void TrackChanged(int index)
        {
            if (songs.Count == 0 || index > songs.Count - 1 || index < 0) return;
            CurrentSong = songs[index];
            await ChangeCover();
            ScrollAfterTrackChanged(index);
        }

        private async Task ChangeCover()
        {
            Cover = await ImagesManager.GetCover(currentSong.Path, false);
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

        private BitmapImage cover = new BitmapImage();
        public BitmapImage Cover
        {
            get { return cover; }
            set { Set(ref cover, value); }
        }

        private SongItem currentSong = new SongItem();
        public SongItem CurrentSong
        {
            get { return currentSong; }
            set { Set(ref currentSong, value); }
        }

        private void UpdatePlaylist()
        {
            Songs = NowPlayingPlaylistManager.Current.songs;
        }

        public async void OnLoaded(ListView p)
        {
            listView = p;
            if (firstVisibleIndex >= songs.Count || firstVisibleIndex < 0)
            {
                firstVisibleIndex = ApplicationSettingsHelper.ReadSongIndex();
                if (firstVisibleIndex >= songs.Count)
                {
                    firstVisibleIndex = 0;
                    //HockeyAdapter.TrackEvent("NowPlayingPlaylistViewModel " + nameof(firstVisibleIndex) + " >=songs.Count");
                }
            }
            if (songs.Count != 0)
            {
                await SetScrollPosition();
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            positionKey = ListViewPersistenceHelper.GetRelativeScrollPosition(listView, ItemToKeyHandler);
            var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
            firstVisibleIndex = isp.FirstVisibleIndex;

            if (suspending)
            {
                pageState[nameof(firstVisibleIndex)] = firstVisibleIndex;
                pageState[nameof(positionKey)] = positionKey;
            }

            return Task.CompletedTask;
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

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            foreach (var s in songs)
            {
                if (s.SongId == ((SongItem)e.ClickedItem).SongId) break;
                index++;
            }
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
        }

        public async void Delete(object sender, RoutedEventArgs e)
        {
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
            CurrentSong.Rating = Int32.Parse(button.Tag.ToString());
            await DatabaseManager.Current.UpdateRatingAsync(currentSong.SongId, currentSong.Rating).ConfigureAwait(false);
        }

        public void SavePlaylist()
        {
            NowPlayingListItem item = new NowPlayingListItem();
            NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }

    }
}
