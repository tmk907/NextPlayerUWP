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
    }
}
