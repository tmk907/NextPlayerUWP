using NextPlayerUWPDataLayer.Helpers;
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
using Template10.Services.NavigationService;
using Windows.UI.Xaml;

namespace NextPlayerUWP.ViewModels
{
    public class PlaylistsViewModel : MusicViewModelBase
    {
        private ObservableCollection<PlaylistItem> playlists = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> Playlists
        {
            get { return playlists; }
            set { Set(ref playlists, value); }
        }

        private string name = "";
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        private PlaylistItem editPlaylist = new PlaylistItem(-1,false,"");
        public PlaylistItem EditPlaylist
        {
            get { return editPlaylist; }
            set { Set(ref editPlaylist, value); }
        }

        protected override async Task LoadData()
        {
            Playlists = await DatabaseManager.Current.GetPlaylistItemsAsync();
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                playlists = new ObservableCollection<PlaylistItem>();
            }
            return base.OnNavigatingFromAsync(args);
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Playlist, ((PlaylistItem)e.ClickedItem).GetParameter());
        }

        public void Save()
        {
            int id = DatabaseManager.Current.InsertPlainPlaylist(name);
            Playlists.Add(new PlaylistItem(id, false, name));
            Name = "";
        }

        public async void ConfirmDelete(object e)
        {
            PlaylistItem p = (PlaylistItem)e;
            if (p.IsSmart)
            {
                if (p.IsNotDefault)
                {
                    Playlists.Remove(p);
                    await DatabaseManager.Current.DeleteSmartPlaylistAsync(p.Id);
                }
            }
            else
            {
                Playlists.Remove(p);
                await DatabaseManager.Current.DeletePlainPlaylistAsync(p.Id);
            }
        }

        public void EditSmartPlaylist(object sender, RoutedEventArgs e)
        {

        }

        public async void SaveEditedName()
        {
            foreach(var p in Playlists)
            {
                if (p.Id == editPlaylist.Id && !p.IsSmart)
                {
                    p.Name = editPlaylist.Name;
                    break;
                }
            }
            await DatabaseManager.Current.UpdatePlaylistName(editPlaylist.Id, editPlaylist.Name);
            //await LoadData();
        }
    }
}
