namespace NextPlayerUWP.Messages
{
    public enum PageNavigatedType
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

    public class PageNavigated
    {
        public bool NavigatedTo { get; set; }
        public PageNavigatedType PageType { get; set; }
    }
}
