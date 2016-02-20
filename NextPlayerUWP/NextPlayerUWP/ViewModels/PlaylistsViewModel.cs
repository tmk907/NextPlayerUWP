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

        private string editName = "";
        public string EditName
        {
            get { return editName; }
            set { Set(ref editName, value); }
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
                if (ApplicationSettingsHelper.PredefinedSmartPlaylistsId().ContainsKey(p.Id))
                {

                }
                else
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

        public void EditPlainPlaylistName(object sender, ItemClickEventArgs e)
        {

        }

        public void EditSmartPlaylist(object sender, ItemClickEventArgs e)
        {

        }

        public void SaveEdit()
        {
            
        }
    }
}
