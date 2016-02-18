using NextPlayerUWP.Common;
using NextPlayerUWP.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class TagsEditorViewModel : Template10.Mvvm.ViewModelBase
    {
        int songId;
        SongData songData;
        string genres;
        string artists;
        string album;

        private Tags tagsData = new Tags();
        public Tags TagsData
        {
            get { if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) tagsData = new Tags();
                return tagsData; }
            set { Set(ref tagsData, value); }
        }

        private bool showProgressBar = false;
        public bool ShowProgressBar
        {
            get { return showProgressBar; }
            set { Set(ref showProgressBar, value); }
        }

        public async void SaveTags(object sender, RoutedEventArgs e)
        {
            ShowProgressBar = true;
            TagsData.FirstArtist = GetFirst(tagsData.Artists);
            TagsData.FirstComposer = GetFirst(tagsData.Composers);
            songData.Tag = TagsData;
            await DatabaseManager.Current.UpdateSongData(songData);
            if (album != tagsData.Album || artists != tagsData.Artists || genres != tagsData.Genres)
            {
                await DatabaseManager.Current.UpdateTables();
            }
            await NowPlayingPlaylistManager.Current.UpdateSong(songData);
            SaveLater.Current.SaveTagsLater(songData);
            App.OnSongUpdated(songData.SongId);
            ShowProgressBar = false;
            NavigationService.GoBack();
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private string GetFirst(string text)
        {
            if (text.IndexOf(';') > 0)
            {
                return text.Substring(0, text.IndexOf(';'));
            }
            return text;
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            songId = -1;
            songData = new SongData();
            if (parameter != null)
            {
                songId = Int32.Parse(MusicItem.ParseParameter(parameter as string)[1]);
                songData = DatabaseManager.Current.GetSongData(songId);
            }
            TagsData = songData.Tag;
            album = tagsData.Album;
            artists = tagsData.Artists;
            genres = tagsData.Genres;
            return Task.CompletedTask;
        }
    }
}
