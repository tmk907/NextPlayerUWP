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
    public class ArtistViewModel : MusicViewModelBase
    {
        private string artistParam;

        private ArtistItem artist = new ArtistItem();
        public ArtistItem Artist
        {
            get { return artist; }
            set { Set(ref artist, value); }
        }

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
            if (!isBack)
            {
                Songs = new ObservableCollection<SongItem>();
            }
            if (parameter != null)
            {
                try
                {
                    var s = parameter.ToString().Split(new string[] { MusicItem.separator }, StringSplitOptions.None);
                    artistParam = s[1];
                }
                catch (Exception ex) { }
            }
        }

        protected override async Task LoadData()
        {
            Artist = await DatabaseManager.Current.GetArtistItemAsync(artistParam);
            Songs = await DatabaseManager.Current.GetSongItemsFromArtistAsync(artistParam);
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

    }
}
