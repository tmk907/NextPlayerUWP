﻿using NextPlayerUWP.Common;
using NextPlayerUWP.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
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

        public async void SaveTags(object sender, RoutedEventArgs e)
        {
            ShowProgressBar = true;
            TagsData.FirstArtist = GetFirst(tagsData.Artists);
            TagsData.FirstComposer = GetFirst(tagsData.Composers);
            songData.Tag = TagsData;
            await DatabaseManager.Current.UpdateSongData(songData);
            if (album != tagsData.Album || albumArtist != tagsData.AlbumArtist )
            {
                AlbumItem a = await DatabaseManager.Current.GetAlbumItemAsync(album, albumArtist);
                AlbumItem a2 = await DatabaseManager.Current.GetAlbumItemAsync(tagsData.Album, tagsData.AlbumArtist);
                if (a.SongsNumber == 1)
                {
                    if (a2.AlbumId > 0)
                    {
                        await DatabaseManager.Current.DeleteAlbumAsync(album, albumArtist);
                    }
                    else
                    {
                        a.Album = tagsData.Album;
                        a.AlbumArtist = tagsData.AlbumArtist;
                        await DatabaseManager.Current.UpdateAlbumItem(a);
                    }
                }
                
                await DatabaseManager.Current.UpdateTables();
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
                    ArtistItem a = await DatabaseManager.Current.GetArtistItemAsync(old[0]);
                    if (a.SongsNumber == 1)
                    {
                        a.Artist = tagsData.Artists;
                        await DatabaseManager.Current.UpdateArtistItem(a);
                    }
                    else
                    {
                        //await DatabaseManager.Current.DeleteArtistAsync(a.Artist);
                    }
                }
                else
                {
                    foreach (var o in old)
                    {
                        bool find = false;
                        foreach (var ed in edited)
                        {
                            if (o.Equals(ed))
                            {
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            ArtistItem a = await DatabaseManager.Current.GetArtistItemAsync(o);
                            if (a.SongsNumber == 1)
                            {
                                await DatabaseManager.Current.DeleteArtistAsync(o);
                            }
                        }
                    }
                }
                
                
                await DatabaseManager.Current.UpdateTables();
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
            albumArtist = tagsData.AlbumArtist;
            return Task.CompletedTask;
        }
    }
}
