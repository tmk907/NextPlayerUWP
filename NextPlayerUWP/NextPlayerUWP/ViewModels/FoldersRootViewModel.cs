using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class FoldersRootViewModel : MusicViewModelBase
    {
        SortingHelperForFolderItems sortingHelper;
        public FoldersRootViewModel()
        {
            sortingHelper = new SortingHelperForFolderItems("FoldersRoot");
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            SortDescending = sortingHelper.SortDescending;
            MediaImport.MediaImported += MediaImport_MediaImported;
            ViewModelLocator vml = new ViewModelLocator();
            helper = vml.FolderVMHelper;
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
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("FoldersRootViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                TelemetryAdapter.TrackEvent("Dispatcher null");
                return;
            }
            await d.DispatchAsync(() => ReloadData());
        }

        private async Task ReloadData()
        {
            folders.Clear();
            await LoadData();
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

        private FoldersVMHelper helper;

        protected override async Task LoadData()
        {
            bool subfolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders);
            FolderName = "";
            if (folders.Count == 0)
            {
                await helper.Initialize();
                Folders = helper.GetRootFolders();
                CloudStorageServiceFactory fact = new CloudStorageServiceFactory();
                var cloudFolders = await fact.GetAllRootFolders();
                foreach (var item in cloudFolders)
                {
                    Folders.Add(item);
                }
            }
            else
            {
                CloudStorageServiceFactory fact = new CloudStorageServiceFactory();
                var cloudFolders = await fact.GetAllRootFolders();

                var toRemove = Folders.OfType<CloudRootFolder>().ToList();

                foreach (var item in toRemove)
                {
                    if (!cloudFolders.Any(c => c.CloudType == item.CloudType && c.UserId == item.UserId))
                    {
                        Folders.Remove(item);
                    }
                }

                foreach (var item in cloudFolders)
                {
                    if (!Folders.OfType<CloudRootFolder>().Any(c => c.CloudType == item.CloudType && c.UserId == item.UserId))
                    {
                        Folders.Add(item);
                    }
                }
            }
            SortMusicItems();
        }

        public override void FreeResources()
        {
            folders = null;
            folders = new ObservableCollection<FolderItem>();
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            if (typeof(FolderItem) == e.ClickedItem.GetType())
            {
                var folder = (FolderItem)e.ClickedItem;
                NavigationService.Navigate(AppPages.Pages.Folders, folder.Directory);
            }
            else if (typeof(CloudRootFolder) == e.ClickedItem.GetType())
            {
                var folder = (CloudRootFolder)e.ClickedItem;
                NavigationService.Navigate(AppPages.Pages.CloudStorageFolders, CloudRootFolder.ToParameter(folder.UserId, folder.CloudType));
            }
        }

        protected override void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            var orderSelector = sortingHelper.GetOrderBySelector();
            var query = (sortDescending) ? folders.OrderByDescending(orderSelector) : folders.OrderBy(orderSelector);
            Folders = new ObservableCollection<FolderItem>(query);
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            //if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            //{
            //    string query = sender.Text.ToLower();
            //    var matchingSongs = items.OfType<SongItem>().Where(s => s.Title.ToLower().StartsWith(query));
            //    var m2 = items.OfType<SongItem>().Where(s => s.Title.ToLower().Contains(query));
            //    var m3 = items.OfType<SongItem>().Where(s => (s.Album.ToLower().Contains(query) || s.Artist.ToLower().Contains(query)));
            //    var m4 = matchingSongs.Concat(m2).Concat(m3).Distinct();
            //    sender.ItemsSource = m4.ToList();
            //}
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
            string id;
            if (args.ChosenSuggestion != null)
            {
                id = ((FolderItem)args.ChosenSuggestion).Directory;
            }
            else
            {
                var list = folders.Where(s => s.Folder.ToLower().StartsWith(args.QueryText.ToLower())).OrderBy(s => s.Folder).ToList();
                if (list.Count == 0) return;
                id = list.FirstOrDefault().Folder;
            }
            int index = 0;
            foreach (var item in folders)
            {
                if (item.GetType() == typeof(FolderItem) && ((FolderItem)item).Folder == id)
                {
                    break;
                }
                index++;
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
