using GalaSoft.MvvmLight.Command;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class FoldersViewModel : MusicViewModelBase
    {
        private ObservableCollection<FolderItem> folders = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> Folders
        {
            get { return folders; }
            set { Set(ref folders, value); }
        }

        protected override async Task LoadData()
        {
            if (Folders.Count == 0)
            {
                Folders = await DatabaseManager.Current.GetFolderItemsAsync();
            }
        }

        private RelayCommand<FolderItem> itemClicked;
        public RelayCommand<FolderItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<FolderItem>(
                    item =>
                    {
                        NavigationService.Navigate(App.Pages.Folders, item.GetParameter());
                    }));
            }
        }
    }
}
