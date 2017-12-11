using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Services
{
    public class LastFmCache
    {
        private CredentialLockerService credentials;
        public LastFmCache()
        {
            credentials = new CredentialLockerService(CredentialLockerService.LastFmVault);
            RefreshCredentialsFromSettings();
        }

        private string Username;
        private string Password;

        private bool IsUserLoggedIn
        {
            get { return Username != "" && Password != ""; }
        }

        private void RefreshCredentialsFromSettings()
        {
            Username = credentials.GetFirstUserName();
            Password = credentials.GetPassword(Username);
        }

        public async Task CacheTrackScrobble(TrackScrobble scrobble)
        {
            RefreshCredentialsFromSettings();
            if (IsUserLoggedIn)
            {
                await DatabaseManager.Current.CacheTrackScrobbleAsync("track.scrobble", scrobble.Artist, scrobble.Track, scrobble.Timestamp);
            }
        }

        public async Task CacheTrackScrobble(SongItem song, TimeSpan playbackDuration, DateTime playedAt)
        {
            if (playbackDuration.TotalSeconds >= song.Duration.TotalSeconds * 0.5 || playbackDuration.TotalSeconds >= 4 * 60)
            {
                RefreshCredentialsFromSettings();
                if (IsUserLoggedIn)
                {
                    string timestamp = ((int)playedAt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                    await DatabaseManager.Current.CacheTrackScrobbleAsync("track.scrobble", song.Artist, song.Title, timestamp);
                }
            }
        }

        public async Task RateSong(string artist, string track, int rating)
        {
            bool rate = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmRateSongs);
            if (rate)
            {
                RefreshCredentialsFromSettings();
                if (IsUserLoggedIn)
                {
                    int min = (int)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmLove);
                    if (rating >= min)
                    {
                        await DatabaseManager.Current.CacheTrackLoveAsync("track.love", artist, track);
                    }
                    else
                    {
                        await DatabaseManager.Current.CacheTrackLoveAsync("track.unlove", artist, track);
                    }
                }
            }
        }
    }
}
