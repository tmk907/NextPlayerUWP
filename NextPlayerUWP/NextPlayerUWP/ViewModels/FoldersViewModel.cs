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
                Folders = await DatabaseManager.Current.GetFolderItemsAsync();
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Playlist, ((FolderItem)e.ClickedItem).GetParameter());
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItemValue value = SelectedComboBoxItem;
            switch (value.Option)
            {
                case SortNames.FolderName:
                    Sort(s => s.Folder, t => (t.Folder == "") ? "" : t.Folder[0].ToString().ToLower(), "Folder");
                    break;
                //case SortNames.Duration:
                //    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "AlbumId");
                //    break;
                case SortNames.SongCount:
                    Sort(s => s.SongsNumber, t => t.SongsNumber, "Folder");
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
                var matchingAlbums = folders.Where(s => s.Folder.ToLower().Contains(sender.Text)).OrderBy(s => s.Folder);
                sender.ItemsSource = matchingAlbums.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                NavigationService.Navigate(App.Pages.Album, ((FolderItem)args.ChosenSuggestion).GetParameter());
            }
            else
            {
                var list = folders.Where(s => s.Folder.ToLower().Contains(sender.Text)).OrderBy(s => s.Folder).ToList();
                //if (list.Count > 0)
                //{
                //    await SongClicked(list.FirstOrDefault().SongId);
                //}
                sender.ItemsSource = list;
            }
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as FolderItem;
            sender.Text = item.Folder;
        }
    }
}
