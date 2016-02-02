using GalaSoft.MvvmLight.Command;
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
    public class AlbumsViewModel : MusicViewModelBase
    {
        private ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();
        public ObservableCollection<AlbumItem> Albums
        {
            get { return albums; }
            set { Set(ref albums, value); }
        }

        private ObservableCollection<GroupList> groupedAlbums = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedAlbums
        {
            get { return groupedAlbums; }
            set { Set(ref groupedAlbums, value); }
        }

        protected override async Task LoadData()
        {
            if (albums.Count == 0)
            {
                Albums = await DatabaseManager.Current.GetAlbumItemsAsync();
            }
            if (groupedAlbums.Count == 0)
            {
                var query = from item in albums
                            group item by item.Album[0] into g
                            orderby g.Key
                            select new { GroupName = g.Key, Items = g };
                foreach (var g in query)
                {
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        group.Add(item);
                    }
                    GroupedAlbums.Add(group);
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!isBack)
            {
                Albums = new ObservableCollection<AlbumItem>();
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Album, ((AlbumItem)e.ClickedItem).GetParameter());
        }
    }
}
