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

namespace NextPlayerUWP.ViewModels
{
    public class FoldersViewModel : MusicViewModelBase
    {
        public FoldersViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.folder);
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
            SortItems(null, null);
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
        }

        string directory;
        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            sortAfterOnNavigated = true;
            
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
                NavigationService.Navigate(App.Pages.Folders, ((FolderItem)e.ClickedItem).Directory);
            }
        }

        private async Task SongClicked(int songid)
        {
            int index = 0;
            int i = 0;
            List<SongItem> songs = new List<SongItem>();
            foreach (var item in items.Where(s=>s.GetType() == typeof(SongItem)))
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

        private bool sortAfterOnNavigated = false;
        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            if (sortAfterOnNavigated)
            {
                sortAfterOnNavigated = false;
                return;
            }
            ComboBoxItemValue value = SelectedComboBoxItem;
            switch (value.Option)
            {
                case SortNames.FolderName:
                    Sort(s => s.Folder, t => (t.Folder == "") ? "" : t.Folder[0].ToString().ToLower(), "Folder");
                    break;
                case SortNames.Directory:
                    Sort(s => s.Directory, t => (t.Directory == "") ? "" : t.Directory[0].ToString().ToLower(), "Folder");
                    break;
                //case SortNames.Duration:
                //    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "AlbumId");
                //    break;
                case SortNames.SongCount:
                    Sort(s => s.SongsNumber, t => t.SongsNumber, "Folder");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.LastAdded.Ticks, t => String.Format("{0:d}", t.LastAdded), "Folder");
                    break;
                default:
                    Sort(s => s.Folder, t => (t.Folder == "") ? "" : t.Folder[0].ToString().ToLower(), "Folder");
                    break;
            }
        }

        private void Sort(Func<FolderItem, object> orderSelector, Func<FolderItem, object> groupSelector, string propertyName)
        {
            var query = folders.OrderBy(orderSelector);
            Folders = new ObservableCollection<FolderItem>(query);
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var matchingFolders = folders.Where(s => s.Folder.ToLower().StartsWith(query)).OrderBy(f => f.Folder);
                var m2 = folders.Where(f => f.Folder.ToLower().Contains(query));
                var m3 = folders.Where(f => f.Directory.ToLower().Contains(query));
                var result = matchingFolders.Concat(m2).Concat(m3).Distinct();
                sender.ItemsSource = result.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int index;
            if (args.ChosenSuggestion != null)
            {
                index = folders.IndexOf((FolderItem)args.ChosenSuggestion);
            }
            else
            {
                var list = folders.Where(s => s.Folder.ToLower().StartsWith(sender.Text.ToLower())).OrderBy(s => s.Folder).ToList();
                if (list.Count == 0) return;
                index = 0;
                bool find = false;
                foreach (var g in folders)
                {
                    if (g.Folder.Equals(list.FirstOrDefault().Folder))
                    {
                        find = true;
                        break;
                    }
                    index++;
                }
                if (!find) return;
                sender.ItemsSource = list;
            }
            listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as FolderItem;
            sender.Text = item.Folder;
        }
    }
}
