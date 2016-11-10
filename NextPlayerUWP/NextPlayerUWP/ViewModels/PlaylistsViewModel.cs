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
using System.IO;

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

        private bool relativePaths = false;
        public bool RelativePaths
        {
            get { return relativePaths; }
            set { Set(ref relativePaths, value); }
        }

        protected override async Task LoadData()
        {
            var p = await DatabaseManager.Current.GetPlaylistItemsAsync();
            if (p.Count != playlists.Count)
            {
                Playlists = p;
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            var item = (PlaylistItem)e.ClickedItem;
            if (item.IsSmart)
            {
                NavigationService.Navigate(App.Pages.Playlist, item.GetParameter());
            }
            else
            {
                NavigationService.Navigate(App.Pages.PlaylistEditable, item.GetParameter());
            }
        }

        public void NewSmartPlaylist()
        {
            NavigationService.Navigate(App.Pages.NewSmartPlaylist);
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
                PlaylistExporter pe = new PlaylistExporter();
                await pe.DeletePlaylist(p).ConfigureAwait(false);
                await DatabaseManager.Current.DeletePlainPlaylistAsync(p.Id).ConfigureAwait(false);
            }
        }

        public void EditSmartPlaylist(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(App.Pages.NewSmartPlaylist,((PlaylistItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter).Id);
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
            PlaylistExporter pe = new PlaylistExporter();
            await pe.ChangePlaylistName(editPlaylist).ConfigureAwait(false);
            //await LoadData();
        }

        public async void ExportPlaylist()
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =  Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("m3u playlist", new List<string>() { ".m3u" });
            savePicker.SuggestedFileName = editPlaylist.Name;

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);

                string folderPath = Path.GetDirectoryName(file.Path);
                PlaylistExporter pe = new PlaylistExporter();
                string content = await pe.ToM3UContent(editPlaylist, relativePaths, folderPath);

                await Windows.Storage.FileIO.WriteTextAsync(file, content);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    //if (editPlaylist.IsSmart)
                    //{
                    //    DatabaseManager.Current.InsertImportedPlaylist(editPlaylist.Name, file.Path, -1);
                    //}
                    //else
                    //{
                    //    DatabaseManager.Current.InsertImportedPlaylist(editPlaylist.Name, file.Path, editPlaylist.Id);
                    //}
                }
                else
                {
                    //"File couldn't be saved.";
                }
            }
            else
            {
                //"Operation cancelled.";
            }
        }
    }
}
