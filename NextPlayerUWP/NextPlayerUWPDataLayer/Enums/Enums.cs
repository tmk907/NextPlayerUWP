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
        LocalNotMusicLibrary = 4
    }
}
