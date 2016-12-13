namespace NextPlayerUWPDataLayer.Constants
{
    public sealed class AppConstants
    {
        //App
        public const string AppName = "Next-Player";
        public const string ProductId = "9nblggh67n4f";
        public const string DBFileName = "database1.db";
        public const string FirstRun = "firstrun";
        public const string DBVersion = "DBVersion";
        public const string AppVersion = "AppVersion";
        public const string DeviceName = "devicename";
        public const string DeveloperEmail = "next-player@outlook.com";
        public const string EnableTelemetry = "EnableTelemetry";
        public const string HockeyAppId = "35b4dc0a397c4b9cb04b958de8947e03 ";

        //Assets
        public const string AlbumCoverSmall = "ms-appx:///Assets/Albums/AlbumCoverTRSmall.png";
        public const string SongCoverBig = "ms-appx:///Assets/Albums/AlbumCoverTR.png";
        public const string AlbumCover = "ms-appx:///Assets/Albums/AlbumCoverTRnG.png";
        public const string AppLogoSmall44 = "ms-appx:///Assets/Visual Assets/Square44/Icon3.png";
        public const string AppLogoSmall71 = "ms-appx:///Assets/Visual Assets/Square71/Small3.png";
        public const string AppLogoMedium = "ms-appx:///Assets/Visual Assets/Square150/Medium3.png";
        public const string AppLogoWide = "ms-appx:///Assets/Visual Assets/Wide310/Wide3.png";
        public const string AppLogoLarge = "ms-appx:///Assets/Visual Assets/Square310/Square310.png";

        public const string RadioCover = "ms-appx:///Assets/Albums/AlbumCoverTRnG.png";

        public const string EmptyMP3File = "ms-appx:///Assets/Sounds/Empty.mp3";

        //Layout
        public const string AppTheme = "AppTheme";
        public const string AppAccent = "AppAccent";
        public const string IsPhoneAccentSet = "IsPhoneAccentSet";
        public const string IsBGImageSet = "IsBGImageSet";
        public const string BackgroundImagePath = "BackgroundImagePath";
        public const string ShowCoverAsBackground = "ShowCoverAsBackground";

        //Review
        public const string IsReviewed = "isreviewed5";
        public const string LastReviewRemind = "lastreviewremind5";

        //BGMessages
        public const string MediaScan = "mediascan";
        public const string NowPlayingListChanged = "nplistchanged";
        public const string NowPlayingListSorted = "nplistsorted";
        public const string NowPlayingListRefresh = "nplistrefresh";

        public const string SongId = "songid";
        public const string SongIndex = "songindex";
        public const string PrevSongIndex = "prevsongindex";
        public const string SkipNext = "skipnext";
        public const string SkipPrevious = "skipprevious";
        public const string StartPlayback = "startplayback";
        public const string Play = "play";
        public const string Pause = "pause";
        public const string Position = "position";
        public const string Repeat = "repeat";
        public const string Shuffle = "shuffle";
        public const string MediaOpened = "mediaopened";
        public const string PlayerClosed = "playerclosed";
        public const string UpdateSongStatistics = "updatesongstatistics";
        public const string ResumePlayback = "resumeplayback";
        public const string SetTimer = "settimer";
        public const string CancelTimer = "canceltimer";
        public const string ShutdownBGPlayer = "shutdown";
        public const string ChangeRate = "changerate";
        public const string UpdateUVC = "updateuvc";
        public const string StreamUpdated = "streamupdated";
        public const string Volume = "volume";

        //Timer
        public const string TimerOn = "TimerOn";
        public const string TimerTime = "TimerTime";
        public const string TimerTaskName = "BackgroundTimer";

        //Toasts
        public const string FilesSharedOK = "FilesSharedOK";
        public const string FilesSharedError = "FilesSharedError";

        //Tiles
        public const string TileName = "TileName";
        public const string TilePlay = "tileplay";
        public const string TileId = "TileId";
        public const string TileType = "TileType";
        public const string TileIdValue = "TileIdValue";
        public const string TileImage = "TileImage";
        public const string TileAppTransparent = "isapptiletransparent";
        public const string EnableLiveTileWithImage = "EnableLiveTileWithImage";

        public const string DataLastSend = "DataLastSend";

        public const string BackgroundTaskStarted = "BackgroundTaskStarted";
        public const string BackgroundTaskRunning = "BackgroundTaskRunning";
        public const string BackgroundTaskCancelled = "BackgroundTaskCancelled";
        public const string BackgroundTaskState = "backgroundtaskstate";
        public const string AppSuspended = "appsuspend";
        public const string AppResumed = "appresumed";
        public const string AppState = "appstate";
        public const string ForegroundAppActive = "Active";
        public const string ForegroundAppSuspended = "Suspended";

        public const string CurrentTrack = "trackname";
        public const string Trackchanged = "songchanged";

        //Default smart playlist names
        public const string OstatnioDodane = "OstatnioDodane";
        public const string OstatnioOdtwarzane = "OstatnioOdtwarzane";
        public const string NajlepiejOceniane = "NajlepiejOceniane";
        public const string NajgorzejOceniane = "NajgorzejOceniane";
        public const string NajczesciejOdtwarzane = "NajczesciejOdtwarzane";
        public const string NajrzadziejOdtwarzane = "NajrzadziejOdtwarzane";

        //Last.fm
        public const string LastFmApiKey = "4737e29d26f813ceeb64aefda42ec0ac";
        public const string LastFmApiSecret = "7b3dbb57b6fda2a24b95270acc77bbc4";
        public const string LfmLogin = "lfmlogin";
        public const string LfmPassword = "lfmpassword";
        public const string LfmSessionKey = "lfmsessionkey";
        public const string LfmRateSongs = "lfmrate";
        public const string LfmLove = "lfmlove";
        public const string LfmSendNP = "lfmsendnp";

        //SoundCloud
        public const string SoundCloudClientId = "1a554caaef0c755f3a794ec4c46d2ee2";
        //public const string SoundCloudClientSecret = "292af0fae1bee4360317164d21cdf8dd";

        //OneDrive
        public const string OneDriveAppId = "0000000048172F05";
        //public const string OneDriveAppSecret = "1VO9tiGE/0apI83XTSl9N9qnp/Pcnn7A";
        //urn:ietf:wg:oauth:2.0:oob
        //https://login.live.com/oauth20_desktop.srf

        //Dropbox
        public const string DropboxAppKey = "cyqbr26o2vtbo8z";
        //public const string DropboxAppSecret = "tlj5wna6iuuhfmm";
        public const string DropboxAuthToken = "DropboxAuthToken";

        //Google Drive
        public const string GoogleDriveClientId = "958984549072-t7lkdmpq50crclpt356js7egc1au3p8u.apps.googleusercontent.com";
        //public const string GoogleDriveAppSecret = "kIPQh3zCiK-X7LsD1lmPBPAJ";

        //pCloud
        public const string PCloudClientId = "5hl070QJ9KY";
        //public const string PCloudAppSecret = "zt17mban1oQj8iq25n5wNbtIuhuV";

        //Jamendo
        public const string JamendoClientId = "8ed1da48";
        //public const string JamendoClientSecret = "45994f52254ba988c15abee8967bbc84";

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
    }
}
