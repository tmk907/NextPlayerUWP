using NextPlayerUWP.Common;
using NextPlayerUWP.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
        string albumArtist;

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

        private bool buttonsEnabled = true;
        public bool ButtonsEnabled
        {
            get { return buttonsEnabled; }
            set { Set(ref buttonsEnabled, value); }
        }

        public async void SaveTags(object sender, RoutedEventArgs e)
        {
            ShowProgressBar = true;
            ButtonsEnabled = false;
            TagsData.FirstArtist = GetFirst(tagsData.Artists);
            TagsData.FirstComposer = GetFirst(tagsData.Composers);
            if (album != tagsData.Album || albumArtist != tagsData.AlbumArtist )
            {
                AlbumItem oldAlbum = await DatabaseManager.Current.GetAlbumItemAsync(album, albumArtist);
                AlbumItem newAlbum = await DatabaseManager.Current.GetAlbumItemAsync(tagsData.Album, tagsData.AlbumArtist);
                //if there is no album with (tagsData.Album, tagsData.AlbumArtist) in db then default AlbumItem is returned and id = -1
                if (oldAlbum.SongsNumber == 1)
                {
                    if (newAlbum.AlbumId > 0)
                    {
                        oldAlbum.LastAdded = (oldAlbum.LastAdded > newAlbum.LastAdded) ? oldAlbum.LastAdded : newAlbum.LastAdded;
                        oldAlbum.ImagePath = newAlbum.ImagePath;
                        oldAlbum.ImageUri = newAlbum.ImageUri;
                        oldAlbum.IsImageSet = newAlbum.IsImageSet;
                        await DatabaseManager.Current.DeleteAlbumAsync(newAlbum.AlbumParam, newAlbum.AlbumArtist);
                    }
                    oldAlbum.AlbumParam = tagsData.Album;
                    oldAlbum.AlbumArtist = tagsData.AlbumArtist;
                    await DatabaseManager.Current.UpdateAlbumItem(oldAlbum);
                }
            }

            if (artists != tagsData.Artists)
            {
                tagsData.Artists = tagsData.Artists.TrimEnd(new char[] { ';', ' ' });
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < tagsData.Artists.Length; i++)
                {
                    sb.Append(tagsData.Artists[i]);
                    if (tagsData.Artists[i]==';' && tagsData.Artists[i+1] != ' ')
                    {
                        sb.Append(' ');
                    }
                }
                tagsData.Artists = sb.ToString();
                var old = artists.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                var edited = tagsData.Artists.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                if (old.Length == 1 && edited.Length == 1)
                {
                    ArtistItem prevArtist = await DatabaseManager.Current.GetArtistItemAsync(old[0]);
                    ArtistItem newArtist = await DatabaseManager.Current.GetArtistItemAsync(edited[0]);
                    if (prevArtist.SongsNumber == 1)
                    {
                        if (newArtist.ArtistId > 0)
                        {
                            prevArtist.LastAdded = (prevArtist.LastAdded > newArtist.LastAdded) ? prevArtist.LastAdded : newArtist.LastAdded;
                            await DatabaseManager.Current.DeleteArtistAsync(edited[0]);
                        }
                        prevArtist.ArtistParam = tagsData.Artists;
                        await DatabaseManager.Current.UpdateArtistItem(prevArtist);
                    }
                }
                //else
                //{
                //    foreach (var o in old)
                //    {
                //        bool find = false;
                //        foreach (var ed in edited)
                //        {
                //            if (o.Equals(ed))
                //            {
                //                find = true;
                //                break;
                //            }
                //        }
                //        if (!find)
                //        {
                //            ArtistItem a = await DatabaseManager.Current.GetArtistItemAsync(o);
                //            if (a.SongsNumber == 1)
                //            {
                //                await DatabaseManager.Current.DeleteArtistAsync(o);
                //            }
                //        }
                //    }
                //}
            }

            if (genres != tagsData.Genres)
            {
                tagsData.Genres = tagsData.Genres.TrimEnd(new char[] { ';', ' ' });
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < tagsData.Genres.Length; i++)
                {
                    sb.Append(tagsData.Genres[i]);
                    if (tagsData.Genres[i] == ';' && tagsData.Genres[i + 1] != ' ')
                    {
                        sb.Append(' ');
                    }
                }
                tagsData.Genres = sb.ToString();
            }
            songData.Tag = TagsData;
            await DatabaseManager.Current.UpdateSongData(songData);
            await DatabaseManager.Current.UpdateTables();
            await NowPlayingPlaylistManager.Current.UpdateSong(songData);
            TagsManager tm = new TagsManager();
            await tm.SaveTags(songData);
            App.OnSongUpdated(songData.SongId);
            ShowProgressBar = false;
            ButtonsEnabled = true;
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

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            songId = -1;
            songData = new SongData();
            if (parameter != null)
            {
                songId = Int32.Parse(MusicItem.SplitParameter(parameter as string)[1]);
                songData = await DatabaseManager.Current.GetSongDataAsync(songId);
            }
            TagsData = songData.Tag;
            album = tagsData.Album;
            artists = tagsData.Artists;
            genres = tagsData.Genres;
            albumArtist = tagsData.AlbumArtist;
            TelemetryAdapter.TrackEvent("Page: Tags Editor");
        }
    }
}
