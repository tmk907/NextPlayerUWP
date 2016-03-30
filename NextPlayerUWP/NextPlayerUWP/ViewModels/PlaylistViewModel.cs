﻿using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using System.IO;

namespace NextPlayerUWP.ViewModels
{
    public class PlaylistViewModel : MusicViewModelBase
    {
        public PlaylistViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.song);
            ComboBoxItemValues = si.GetSortNames();
            SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                playlist = new ObservableCollection<SongItem>();
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
            }
        }

        private MusicItemTypes type;
        string firstParam;

        private ObservableCollection<SongItem> playlist = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Playlist
        {
            get { return playlist; }
            set { Set(ref playlist, value); }
        }

        private string pageSubTitle = "";
        public string PageSubTitle
        {
            get { return pageSubTitle; }
            set { Set(ref pageSubTitle, value); }
        }

        protected override async Task LoadData()
        {
            if (Playlist.Count == 0)
            {
                PlaylistItem p = new PlaylistItem(-1, false, "Playlist");
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                switch (type)
                {
                    case MusicItemTypes.folder:
                        PageTitle = loader.GetString("Folder");
                        PageSubTitle = Path.GetFileName(firstParam);
                        Playlist = await DatabaseManager.Current.GetSongItemsFromFolderAsync(firstParam);
                        break;
                    case MusicItemTypes.genre:
                        PageTitle = loader.GetString("Genre");
                        PageSubTitle = firstParam;
                        Playlist = await DatabaseManager.Current.GetSongItemsFromGenreAsync(firstParam);
                        break;
                    case MusicItemTypes.plainplaylist:
                        PageTitle = loader.GetString("Playlist");
                        p = await DatabaseManager.Current.GetPlainPlaylistAsync(Int32.Parse(firstParam));
                        PageSubTitle = p.Name;
                        Playlist = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(Int32.Parse(firstParam));
                        break;
                    case MusicItemTypes.smartplaylist:
                        PageTitle = loader.GetString("Playlist");
                        p = await DatabaseManager.Current.GetSmartPlaylistAsync(Int32.Parse(firstParam));
                        PageSubTitle = p.Name;
                        Playlist = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(Int32.Parse(firstParam));
                        break;
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                type = MusicItem.ParseType(parameter as string);
                firstParam = MusicItem.ParseParameter(parameter as string)[1];
            }
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                playlist = new ObservableCollection<SongItem>();
                pageTitle = "";
            }
            return base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            await SongClicked(((SongItem)e.ClickedItem).SongId);
        }

        private async Task SongClicked(int songid)
        {
            int index = 0;
            foreach (var s in playlist)
            {
                if (s.SongId == songid) break;
                index++;
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(playlist);
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItemValue value = SelectedComboBoxItem;
            switch (value.Option)
            {
                case SortNames.Title:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
                case SortNames.Album:
                    Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower(), "Album");
                    break;
                case SortNames.Artist:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
                case SortNames.AlbumArtist:
                    Sort(s => s.AlbumArtist, t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower(), "AlbumArtist");
                    break;
                case SortNames.Year:
                    Sort(s => s.Year, t => t.Year, "SongId");
                    break;
                case SortNames.Duration:
                    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "SongId");
                    break;
                case SortNames.Rating:
                    Sort(s => s.Rating, t => t.Rating, "SongId");
                    break;
                case SortNames.Composer:
                    Sort(s => s.Composer, t => (t.Composer == "") ? "" : t.Composer[0].ToString().ToLower(), "Composer");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.DateAdded, t => String.Format("{0:d}", t.DateAdded), "SongId");
                    break;
                case SortNames.LastPlayed:
                    Sort(s => s.LastPlayed, t => String.Format("{0:d}", t.DateAdded), "SongId");
                    break;
                case SortNames.PlayCount:
                    Sort(s => s.PlayCount, t => t.PlayCount, "SongId");
                    break;
                default:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
            }
        }

        private void Sort(Func<SongItem, object> orderSelector, Func<SongItem, object> groupSelector, string propertyName)
        {
            var query = playlist.OrderBy(orderSelector);
            Playlist = new ObservableCollection<SongItem>(query);
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var matchingSongs = playlist.Where(s => s.Title.ToLower().StartsWith(sender.Text)).OrderBy(s => s.Title);
                sender.ItemsSource = matchingSongs.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int index;
            if (args.ChosenSuggestion != null)
            {
                index = playlist.IndexOf((SongItem)args.ChosenSuggestion);
            }
            else
            {
                var list = playlist.Where(s => s.Title.ToLower().StartsWith(sender.Text)).OrderBy(s => s.Title).ToList();
                if (list.Count == 0) return;
                index = 0;
                bool find = false;
                foreach (var g in playlist)
                {
                    if (g.Title.Equals(list.FirstOrDefault().Title))
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
            var song = args.SelectedItem as SongItem;
            sender.Text = song.Title;
        }
    }
}
