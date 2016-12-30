using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
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
        SortingHelperForSongItems sortingHelper;
        public FoldersViewModel()
        {
            sortingHelper = new SortingHelperForSongItems("Folders");
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            MediaImport.MediaImported += MediaImport_MediaImported;
        }

        private async void MediaImport_MediaImported(string s)
        {
            Template10.Common.IDispatcherWrapper d = Dispatcher;
            if (d == null)
            {
                d = Template10.Common.WindowWrapper.Current().Dispatcher;
            }
            if (d == null)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("FoldersViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                return;
            }
            await d.DispatchAsync(() => ReloadData());
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
            if (items.Count != 0)
            {
                items = new ObservableCollection<MusicItem>();
            }

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
            if (directory == null)
            {
                CloudStorageServiceFactory fact = new CloudStorageServiceFactory();
                var items = await fact.GetAllRootFolders();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            SortMusicItems();
        }

        public override void FreeResources()
        {
            folders = null;
            items = null;
            folders = new ObservableCollection<FolderItem>();
            items = new ObservableCollection<MusicItem>();
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
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            var orderSelector = sortingHelper.GetOrderBySelector();

            var folderItems = items.Where(i => i.GetType() != typeof(SongItem));
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
