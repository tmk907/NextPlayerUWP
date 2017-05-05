namespace NextPlayerUWPDataLayer.Helpers
{
    public sealed class SettingsKeys
    {
        public const string FirstRun = "firstrun";
        public const string DBVersion = "DBVersion";
        public const string EnableTelemetry = "EnableTelemetry";
        public const string AppVersion = "AppVersion";
        public const string DeviceName = "devicename";

        //Last.fm
        public const string LfmLogin = "lfmlogin";
        public const string LfmPassword = "lfmpassword";
        public const string LfmSessionKey = "lfmsessionkey";
        public const string LfmRateSongs = "lfmrate";
        public const string LfmLove = "lfmlove";
        public const string LfmSendNP = "lfmsendnp";

        //Layout
        public const string AppTheme = "AppTheme";
        public const string AppAccent = "AppAccent";
        public const string AccentFromAlbumArt = "AccentFromAlbumArt";
        public const string AlbumArtInBackground = "AlbumArtInBackground";

        //Review
        public const string IsReviewed = "isreviewed5";
        public const string LastReviewRemind = "lastreviewremind5";

        //Timer
        public const string TimerOn = "TimerOn";
        public const string TimerTime = "TimerTime";

        //Tiles
        public const string TileName = "TileName";
        public const string TileId = "TileId";
        public const string TileType = "TileType";
        public const string TileIdValue = "TileIdValue";
        public const string TileImage = "TileImage";
        public const string TileAppTransparent = "isapptiletransparent";
        public const string EnableLiveTileWithImage = "EnableLiveTileWithImage";


        public const string MediaScan = "mediascan";

        public const string SongIndex = "songindex";
        public const string PrevSongIndex = "prevsongindex";
        public const string Repeat = "repeat";
        public const string Shuffle = "shuffle";
        public const string Volume = "volume";


        public const string ActionAfterDropItem = "ActionAfterDropItem";
        public const string ActionPlayNext = "ActionPlayNext";
        public const string ActionAddToNowPlaying = "ActionAddToNowPlaying";
        public const string ActionPlayNow = "ActionPlayNow";

        public const string ActionAfterSwipeLeftCommand = "ActionAfterSwipeLeftCommand";
        public const string ActionAfterSwipeRightCommand = "ActionAfterSwipeRightCommand";
        public const string SwipeActionPlayNext = "SwipeActionPlayNext";
        public const string SwipeActionAddToNowPlaying = "SwipeActionAddToNowPlaying";
        public const string SwipeActionPlayNow = "SwipeActionPlayNow";
        public const string SwipeActionAddToPlaylist = "SwipeActionAddToPlaylist";
        public const string SwipeActionDelete = "SwipeActionDelete";


        public const string DisableLockscreen = "DisableLockscreen";
        public const string HideStatusBar = "HideStatusBar";
        public const string IncludeSubFolders = "IncludeSubFolders";
        public const string PlaylistsFolder = "PlaylistsFolder";
        public const string AutoSavePlaylists = "AutoSavePlaylists";
        public const string FlipViewSelectedIndex = "FlipViewSelectedIndex";

        public const string LibraryUpdatedAt = "LibraryUpdatedAt";
        public const string LibraryUpdateFrequency = "LibraryUpdateFrequency";

        public const string IgnoreArticles = "IgnoreArticles";
        public const string IgnoredArticlesList = "IgnoredArticlesList";

        public const string MenuEntries = "MenuEntries";

        public const string ExtensionsLyrics = "LyricsExtensions";
        public const string ExtensionsArtistInfo = "ExtensionsArtistInfo";
        public const string ExtensionsAlbumInfo = "ExtensionsAlbumInfo";
    }
}
