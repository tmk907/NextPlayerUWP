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
    public class ArtistsViewModel : MusicViewModelBase
    {
        private ObservableCollection<ArtistItem> artists = new ObservableCollection<ArtistItem>();
        public ObservableCollection<ArtistItem> Artists
        {
            get { return artists; }
            set { Set(ref artists, value); }
        }

        private ObservableCollection<GroupList> groupedArtists = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedArtists
        {
            get { return groupedArtists; }
            set { Set(ref groupedArtists, value); }
        }

        protected override async Task LoadData()
        {
            if (Artists.Count == 0)
            {
                Artists = await DatabaseManager.Current.GetArtistItemsAsync();
            }
            if (GroupedArtists.Count == 0)
            {
                var query = from item in artists
                            group item by item.Artist[0] into g
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
                    GroupedArtists.Add(group);
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!isBack)
            {
                Artists = new ObservableCollection<ArtistItem>();
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Artist, ((ArtistItem)e.ClickedItem).GetParameter());
        }
    }
}
