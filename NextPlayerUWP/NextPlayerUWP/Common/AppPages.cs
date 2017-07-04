using NextPlayerUWP.Views;
using System;
using System.Collections.Generic;

namespace NextPlayerUWP.Common
{
    public class AppPages
    {
        public enum Pages
        {
            AddToPlaylist,
            Albums,
            Album,
            AlbumArtists,
            AlbumArtist,
            Artists,
            Artist,
            AudioSettings,
            CloudStorageFolders,
            CuteRadio,
            Genres,
            FileInfo,
            Folders,
            FoldersRoot,
            Lyrics,
            Licenses,
            NewSmartPlaylist,
            NowPlaying,
            NowPlayingDesktop,
            NowPlayingPlaylist,
            Playlists,
            Playlist,
            PlaylistEditable,
            Radios,
            Settings,
            Songs,
            TagsEditor
        }

        public readonly Dictionary<Pages, string> PagesToKeys = new Dictionary<Pages, string>()
        {
            { Pages.AddToPlaylist, "AddToPlaylist" },
            { Pages.Album, "Album" },
            { Pages.AlbumArtist, "AlbumArtist" },
            { Pages.AlbumArtists, "AlbumArtists" },
            { Pages.Albums, "Albums" },
            { Pages.Artist, "Artist" },
            { Pages.Artists, "Artists" },
            { Pages.AudioSettings, "AudioSettings" },
            { Pages.CloudStorageFolders, "CloudStorageFolders" },
            { Pages.CuteRadio, "CuteRadio" },
            { Pages.FileInfo, "FileInfo" },
            { Pages.Folders, "Folders" },
            { Pages.FoldersRoot, "FoldersRoot" },
            { Pages.Genres, "Genres" },
            { Pages.Licenses, "Licenses" },
            { Pages.Lyrics, "Lyrics" },
            { Pages.NewSmartPlaylist, "NewSmartPlaylist" },
            { Pages.NowPlaying, "NowPlaying" },
            { Pages.NowPlayingDesktop, "NowPlayingDesktop" },
            { Pages.NowPlayingPlaylist, "NowPlayingPlaylist" },
            { Pages.Playlist, "Playlist" },
            { Pages.PlaylistEditable, "PlaylistEditable" },
            { Pages.Playlists, "Playlists" },
            { Pages.Radios, "Radios" },
            { Pages.Settings, "Settings" },
            { Pages.Songs, "Songs" },
            { Pages.TagsEditor, "TagsEditor" },
        };

        public readonly Dictionary<Pages, Type> PagesToTypes = new Dictionary<Pages, Type>()
        {
            { Pages.AddToPlaylist, typeof(AddToPlaylistView) },
            { Pages.Album, typeof(AlbumView) },
            { Pages.AlbumArtist, typeof(AlbumArtistView) },
            { Pages.AlbumArtists, typeof(AlbumArtistsView) },
            { Pages.Albums, typeof(AlbumsView) },
            { Pages.Artist, typeof(ArtistView) },
            { Pages.Artists, typeof(ArtistsView) },
            { Pages.AudioSettings, typeof(AudioSettingsView) },
            { Pages.CloudStorageFolders, typeof(CloudStorageFoldersView) },
            { Pages.CuteRadio, typeof(CuteRadioView) },
            { Pages.FileInfo, typeof(FileInfoView) },
            { Pages.Folders, typeof(FoldersView) },
            { Pages.FoldersRoot, typeof(FoldersRootView) },
            { Pages.Genres, typeof(GenresView) },
            { Pages.Licenses, typeof(Licences) },
            { Pages.Lyrics, typeof(LyricsView) },
            { Pages.NewSmartPlaylist, typeof(NewSmartPlaylistView) },
            { Pages.NowPlaying, typeof(NowPlayingView) },
            { Pages.NowPlayingDesktop, typeof(NowPlayingDesktopView) },
            { Pages.NowPlayingPlaylist, typeof(NowPlayingPlaylistView) },
            { Pages.Playlist, typeof(PlaylistView) },
            { Pages.PlaylistEditable, typeof(PlaylistEditableView) },
            { Pages.Playlists, typeof(PlaylistsView) },
            { Pages.Radios, typeof(RadiosView) },
            { Pages.Settings, typeof(SettingsView2) },
            { Pages.Songs, typeof(SongsView) },
            { Pages.TagsEditor, typeof(TagsEditor) },
        };
    }
}
