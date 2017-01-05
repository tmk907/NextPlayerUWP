namespace NextPlayerUWPDataLayer.Enums
{
    public enum AppState
    {
        Unknown,
        Active,
        Suspended
    }

    public enum BackgroundTaskState
    {
        Unknown,
        Started,
        Running,
        Canceled
    }

    public enum AppTheme
    {
        Light,
        Dark
    }

    public enum RadioType
    {
        Jamendo,
        Unknown
    }

    public enum MusicSource
    {
        LocalFile = 1,
        RadioJamendo = 2,
        OnlineFile = 3,
        LocalNotMusicLibrary = 4,
        OneDrive = 5,
        Dropbox = 6,
        GoogleDrive = 7,
        PCloud = 8,
        Unknown = 9,
    }
}
