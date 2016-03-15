using GalaSoft.MvvmLight.Command;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

        private ObservableCollection<FolderItem> folders = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> Folders
        {
            get { return folders; }
            set { Set(ref folders, value); }
        }

        protected override async Task LoadData()
        {
            if (folders.Count == 0)
            {
                folders = await DatabaseManager.Current.GetFolderItemsAsync();
                SortItems(null, null);
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            sortAfterOnNavigated = true;
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Playlist, ((FolderItem)e.ClickedItem).GetParameter());
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
                    Sort(s => s.LastAdded, t => String.Format("{0:d}", t.LastAdded), "Folder");
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
                var matchingFolders = folders.Where(s => s.Folder.ToLower().StartsWith(sender.Text.ToLower())).OrderBy(f => f.Folder);
                sender.ItemsSource = matchingFolders.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int index;
            if (args.ChosenSuggestion != null)
            {
                index = folders.IndexOf((FolderItem)args.ChosenSuggestion);
                //NavigationService.Navigate(App.Pages.Album, ((FolderItem)args.ChosenSuggestion).GetParameter());
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
