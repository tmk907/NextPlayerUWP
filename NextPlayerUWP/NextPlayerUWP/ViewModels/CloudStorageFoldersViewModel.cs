using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class CloudStorageFoldersViewModel : MusicViewModelBase
    {
        public CloudStorageFoldersViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.song);
            ComboBoxItemValues = si.GetSortNames();
            SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault();
        }

        private string pageHeader = "";
        public string PageHeader
        {
            get { return pageHeader; }
            set { Set(ref pageHeader, value); }
        }

        private string folderName = "";
        public string FolderName
        {
            get { return folderName; }
            set { Set(ref folderName, value); }
        }

        private ObservableCollection<MusicItem> items = new ObservableCollection<MusicItem>();
        public ObservableCollection<MusicItem> Items
        {
            get { return items; }
            set { Set(ref items, value); }
        }

        private CloudFolder currentFolder;

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { Set(ref loading, value); }
        }

        string folderId = "";
        ICloudStorageService service;

        protected override async Task LoadData()
        {
            Loading = true;
            currentFolder = null;
            if (folderId == null)
            {
                var loggedIn = await service.LoginSilently();
                if (!loggedIn)
                {
                    Items.Clear();
                    Loading = false;
                    return;
                }
                folderId = await service.GetRootFolderId();
                if (folderId == null)
                {
                    Items.Clear();
                    Loading = false;
                    return;
                }
            }

            currentFolder = await service.GetFolder(folderId);

            FolderName = currentFolder?.Folder ?? "";

            var folders = await service.GetSubFolders(folderId);
            var songs = await service.GetSongItems(folderId);
            Items.Clear();
            foreach (var folder in folders)
            {
                Items.Add(folder);
            }
            foreach (var song in songs)
            {
                Items.Add(song);
            }
            SortMusicItems();
            Loading = false;
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            string param = parameter as string ?? "";
            if (CloudRootFolder.IsCloudRootFolderParameter(param))
            {
                Items.Clear();
                string userId = CloudRootFolder.ParameterToUserId(param);
                var type = CloudRootFolder.ParameterToType(param);
                switch (type)
                {
                    case CloudStorageType.Dropbox:
                        PageHeader = "Dropbox";
                        break;
                    case CloudStorageType.GoogleDrive:
                        PageHeader = "Google Drive";
                        break;
                    case CloudStorageType.OneDrive:
                        PageHeader = "OneDrive";
                        break;
                    case CloudStorageType.pCloud:
                        PageHeader = "pCloud";
                        break;
                    default:
                        PageHeader = "Unknown Cloud Storage";
                        break;
                }
                CloudStorageServiceFactory factory = new CloudStorageServiceFactory();
                service = factory.GetService(type, userId);
                folderId = null;
            }
            else
            {
                folderId = param;
            }
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            items = new ObservableCollection<MusicItem>();
            //if (args.NavigationMode == NavigationMode.Back && FolderName != "OneDrive Music")//zmienic na spr parentid
            //{
            //    //FolderName.Substring(0,FolderName.LastIndexOf('\\'));
            //}
            await base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            if (typeof(SongItem) == e.ClickedItem.GetType())
            {
                await SongClicked(((SongItem)e.ClickedItem).SongId);
            }
            else if (typeof(CloudFolder) == e.ClickedItem.GetType())
            {
                var folder = ((CloudFolder)e.ClickedItem);
                //FolderName += @"\" + folder.Folder;
                NavigationService.Navigate(App.Pages.CloudStorageFolders, folder.Id);
            }
        }

        private async Task SongClicked(int songid)
        {
            int index = 0;
            int i = 0;
            List<SongItem> songs = new List<SongItem>();
            foreach (var item in items.Where(s => s.GetType() == typeof(SongItem)))
            {
                if (typeof(SongItem) == item.GetType())
                {
                    songs.Add((SongItem)item);
                    if (((SongItem)item).SongId == songid) index = i;
                    i++;
                }
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(songs);
            ApplicationSettingsHelper.SaveSongIndex(index);
            App.PlaybackManager.PlayNew();
        }

        protected override void SortMusicItems()
        {
            string option = selectedComboBoxItem.Option;
            switch (option)
            {
                case SortNames.Title:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
                case SortNames.Album:
                    Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower(), "Album");
                    break;
                case SortNames.Artist:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
                case SortNames.AlbumArtist:
                    Sort(s => s.AlbumArtist, t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower(), "AlbumArtist");
                    break;
                case SortNames.Year:
                    Sort(s => s.Year, t => t.Year, "SongId");
                    break;
                case SortNames.Duration:
                    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "SongId");
                    break;
                case SortNames.Rating:
                    Sort(s => s.Rating, t => t.Rating, "SongId");
                    break;
                case SortNames.Composer:
                    Sort(s => s.Composer, t => (t.Composer == "") ? "" : t.Composer[0].ToString().ToLower(), "Composer");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.DateAdded.Ticks, t => String.Format("{0:d}", t.DateAdded), "SongId");
                    break;
                case SortNames.LastPlayed:
                    Sort(s => s.LastPlayed.Ticks, t => String.Format("{0:d}", t.LastPlayed), "SongId");
                    break;
                case SortNames.PlayCount:
                    Sort(s => s.PlayCount, t => t.PlayCount, "SongId");
                    break;
                case SortNames.TrackNumber:
                    Sort(s => s.TrackNumber, s => s.TrackNumber, "SongId");
                    break;
                default:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
            }
        }

        private void Sort(Func<SongItem, object> orderSelector, Func<SongItem, object> groupSelector, string propertyName)
        {
            var folderItems = items.Where(i => i.GetType() == typeof(CloudFolder));

            var sortedSongs = items.OfType<SongItem>().OrderBy(orderSelector);

            Items = new ObservableCollection<MusicItem>(folderItems);
            foreach (var song in sortedSongs)
            {
                Items.Add(song);
            }
        }


        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var matchingSongs = items.OfType<SongItem>().Where(s => s.Title.ToLower().StartsWith(query));
                var m2 = items.OfType<SongItem>().Where(s => s.Title.ToLower().Contains(query));
                var m3 = items.OfType<SongItem>().Where(s => (s.Album.ToLower().Contains(query) || s.Artist.ToLower().Contains(query)));
                var m4 = matchingSongs.Concat(m2).Concat(m3).Distinct();
                sender.ItemsSource = m4.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int id;
            if (args.ChosenSuggestion != null)
            {
                id = ((SongItem)args.ChosenSuggestion).SongId;
            }
            else
            {
                var list = items.OfType<SongItem>().Where(s => s.Title.ToLower().StartsWith(args.QueryText.ToLower())).OrderBy(s => s.Title).ToList();
                if (list.Count == 0) return;
                id = list.FirstOrDefault().SongId;
            }
            int index = 0;
            foreach (var item in items)
            {
                if (item.GetType() == typeof(SongItem) && ((SongItem)item).SongId == id)
                {
                    break;
                }
                index++;
            }
            listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var song = args.SelectedItem as SongItem;
            sender.Text = song.Title;
        }
    }
}
