using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.CloudStorage;

namespace NextPlayerUWP.ViewModels
{
    public class FoldersViewModel : MusicViewModelBase
    {
        public FoldersViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.song);
            ComboBoxItemValues = si.GetSortNames();
            SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault();
            MediaImport.MediaImported += MediaImport_MediaImported;
        }

        private async void MediaImport_MediaImported(string s)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        private async Task ReloadData()
        {
            Folders = await DatabaseManager.Current.GetFolderItemsAsync();
            SortMusicItems();
        }

        private string folderName = "";
        public string FolderName
        {
            get { return folderName; }
            set { Set(ref folderName, value); }
        }

        private ObservableCollection<FolderItem> folders = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> Folders
        {
            get { return folders; }
            set { Set(ref folders, value); }
        }

        private ObservableCollection<MusicItem> items = new ObservableCollection<MusicItem>();
        public ObservableCollection<MusicItem> Items
        {
            get { return items; }
            set { Set(ref items, value); }
        }

        protected override async Task LoadData()
        {
            bool subfolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders);
            FolderName = directory ?? "";
            //if (folders.Count == 0)
            //{
            Folders = await DatabaseManager.Current.GetFolderItemsAsync();
                //SortItems(null, null);
            //}
            if (folders.Count > 0)
            {
                if (directory == null)
                {
                    string dir = "!";
                    List<string> roots = new List<string>();
                    //var f1 = folders.OrderBy(f => f.Directory).FirstOrDefault();
                    foreach(var f1 in folders.OrderBy(f => f.Directory))
                    {
                        if (!f1.Directory.StartsWith(dir))
                        {
                            if (subfolders) f1.SongsNumber = folders.Where(f => f.Directory.StartsWith(f1.Directory)).Sum(g => g.SongsNumber);
                            Items.Add(f1);
                            dir = f1.Directory;
                            roots.Add(dir);
                        }
                    }
                    CloudStorageServiceFactory fact = new CloudStorageServiceFactory();
                    var items = await fact.GetAllRootFolders();
                    foreach(var item in items)
                    {
                        Items.Add(item);
                    }
                }
                else
                {
                    int numberOfSeparators = directory.Count(c=>c.Equals('\\')) + 1;
                    var f2 = folders.Where(f => (f.Directory.StartsWith(directory+@"\") && f.Directory.Count(c => c.Equals('\\')) == numberOfSeparators));
                    foreach(var f3 in f2)
                    {
                        if (subfolders) f3.SongsNumber = folders.Where(f => f.Directory.StartsWith(f3.Directory)).Sum(g => g.SongsNumber);
                        Items.Add(f3);
                    }
                    var s1 = await DatabaseManager.Current.GetSongItemsFromFolderAsync(directory);
                    foreach (var s in s1)
                    {
                        Items.Add(s);
                    }
                }
            }
            SortMusicItems();
        }

        string directory;
        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {   
            directory = parameter as string;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            items = new ObservableCollection<MusicItem>();

            await base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            if (typeof(SongItem) == e.ClickedItem.GetType())
            {
                await SongClicked(((SongItem)e.ClickedItem).SongId);
            }
            else if (typeof(FolderItem) == e.ClickedItem.GetType())
            {
                var folder = (FolderItem)e.ClickedItem;
                NavigationService.Navigate(App.Pages.Folders, folder.Directory);
            }
            else if (typeof(CloudRootFolder) == e.ClickedItem.GetType())
            {
                var folder = (CloudRootFolder)e.ClickedItem;
                NavigationService.Navigate(App.Pages.CloudStorageFolders, CloudRootFolder.ToParameter(folder.UserId, folder.CloudType));
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
            await PlaybackService.Instance.PlayNewList(index);
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
                case SortNames.FileName:
                    Sort(s => s.FileName, s => s.FileName[0].ToString().ToLower(), "FileName");
                    break;
                default:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
            }
        }

        private void Sort(Func<SongItem, object> orderSelector, Func<SongItem, object> groupSelector, string propertyName)
        {
            var folderItems = items.Where(i => i.GetType() != typeof(SongItem));
            
            var sortedSongs = items.OfType<SongItem>().OrderBy(orderSelector);

            Items = new ObservableCollection<MusicItem>(folderItems);
            foreach(var song in sortedSongs)
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
            //if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            //{
            //    string query = sender.Text.ToLower();
            //    var matchingFolders = folders.Where(s => s.Folder.ToLower().StartsWith(query)).OrderBy(f => f.Folder);
            //    var m2 = folders.Where(f => f.Folder.ToLower().Contains(query));
            //    var m3 = folders.Where(f => f.Directory.ToLower().Contains(query));
            //    var result = matchingFolders.Concat(m2).Concat(m3).Distinct();
            //    sender.ItemsSource = result.ToList();
            //}
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
                if (item.GetType() == typeof(SongItem) &&  ((SongItem)item).SongId == id)
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
