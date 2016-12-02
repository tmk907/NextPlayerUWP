using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Provider;
using NextPlayerUWPDataLayer.Playlists;
using System.Linq;

namespace NextPlayerUWP.ViewModels
{
    public class PlaylistsViewModel : MusicViewModelBase
    {
        private ObservableCollection<PlaylistItem> allPlaylists = new ObservableCollection<PlaylistItem>();
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

        private PlaylistItem editPlaylist = new PlaylistItem(-1, false, "");
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

        private ObservableCollection<PlaylistFilterElement> filters = new ObservableCollection<PlaylistFilterElement>();
        public ObservableCollection<PlaylistFilterElement> Filters
        {
            get { return filters; }
            set { Set(ref filters, value); }
        }

        protected override async Task LoadData()
        {
            var p = await DatabaseManager.Current.GetPlaylistItemsAsync();
            if (p.Count != allPlaylists.Count)
            {
                Playlists.Clear();
                allPlaylists = p;
                foreach(var item in allPlaylists.Where(i => !i.IsHidden))
                {
                    Playlists.Add(item);
                }
            }
            if (filters.Count == 0)
            {
                Filters.Add(new PlaylistFilterElement(FilterPlaylists)
                {
                    IsChecked = true,
                    Name = "Smart playlists"
                });
                Filters.Add(new PlaylistFilterElement(FilterPlaylists)
                {
                    IsChecked = true,
                    Name = "Normal playlists"
                });
                Filters.Add(new PlaylistFilterElement(FilterPlaylists)
                {
                    IsChecked = false,
                    Name = "Show hidden"
                });
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

        public async void Save()
        {
            int id = DatabaseManager.Current.InsertPlainPlaylist(name);
            var playlist = await DatabaseManager.Current.GetPlainPlaylistAsync(id);
            Playlists.Add(playlist);
            Name = "";
        }

        public async void ConfirmDelete(object e)
        {
            PlaylistItem p = (PlaylistItem)e;
            Playlists.Remove(p);
            PlaylistHelper ph = new PlaylistHelper();
            await ph.DeletePlaylistItem(p);
        }

        public void EditSmartPlaylist(object sender, RoutedEventArgs e)
        {
            var playlist = (PlaylistItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.NewSmartPlaylist,playlist.Id);
        }

        public async void SaveEditedName()
        {
            foreach(var p in Playlists)
            {
                if (p.Id == editPlaylist.Id && !p.IsSmart)
                {
                    PlaylistHelper ph = new PlaylistHelper();
                    await ph.EditName(p,editPlaylist.Name);
                    break;
                }
            }
            //await LoadData();
        }

        public void FilterPlaylists()
        {
            if (filters.Count < 3) return;
            bool showSmart = filters[0].IsChecked;
            bool showNormal = filters[1].IsChecked;
            bool showHidden = filters[2].IsChecked;
            Playlists = new ObservableCollection<PlaylistItem>(allPlaylists.Where(p => ((p.IsSmart && showSmart) || (!p.IsSmart && showNormal)) && (!p.IsHidden || showHidden)));
        }

        public void ShowAllPlaylists()
        {
            Playlists = new ObservableCollection<PlaylistItem>(allPlaylists);
        }

        public async void ShowPlaylist(object sender, RoutedEventArgs e)
        {
            var playlist = (PlaylistItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            PlaylistHelper ph = new PlaylistHelper();
            await ph.EditPlaylist(playlist, false);
            Playlists.Insert(allPlaylists.IndexOf(playlist), playlist);
        }

        public async void HidePlaylist(object sender, RoutedEventArgs e)
        {
            var playlist = (PlaylistItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            PlaylistHelper ph = new PlaylistHelper();
            await ph.EditPlaylist(playlist, true);
            Playlists.Remove(playlist);
        }

        

        public async void ExportPlaylist()
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =  Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("m3u playlist", new List<string>() { ".m3u" });
            savePicker.FileTypeChoices.Add("pls playlist", new List<string>() { ".pls" });
            savePicker.FileTypeChoices.Add("wpl playlist", new List<string>() { ".wpl" });
            savePicker.FileTypeChoices.Add("zpl playlist", new List<string>() { ".zpl" });
            savePicker.SuggestedFileName = editPlaylist.Name;

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                PlaylistHelper ph = new PlaylistHelper();
                await ph.ExportPlaylistToFile(editPlaylist, file, false, relativePaths);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    
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

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var matching = playlists.Where(s => s.Name.ToLower().StartsWith(query));
                sender.ItemsSource = matching.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int index;
            if (args.ChosenSuggestion != null)
            {
                index = playlists.IndexOf((PlaylistItem)args.ChosenSuggestion);
            }
            else
            {
                var list = playlists.Where(s => s.Name.ToLower().StartsWith(sender.Text)).OrderBy(s => s.Name).ToList();
                if (list.Count == 0) return;
                index = 0;
                bool find = false;
                foreach (var g in playlists)
                {
                    if (g.Name.Equals(list.FirstOrDefault().Name))
                    {
                        find = true;
                        break;
                    }
                    index++;
                }
                if (!find) return;
            }
            listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as PlaylistItem;
            sender.Text = item.Name;
        }
    }
}
