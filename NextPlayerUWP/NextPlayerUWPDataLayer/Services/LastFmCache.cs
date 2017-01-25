using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Services
{
    public class LastFmCache
    {
        public LastFmCache()
        {
            Username = (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmLogin) ?? String.Empty).ToString();
            Password = (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmPassword) ?? String.Empty).ToString();
        }

        private string Username;
        private string Password;

        public bool AreCredentialsSet()
        {
            return Username != "" && Password != "";
        }

        private void Login()
        {
            Username = "a";
            Password = "a";
        }

        private void Logout()
        {
            Username = "";
            Password = "";
        }

        public void GetCredentialsFromSettings()
        {
            Username = (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmLogin) ?? String.Empty).ToString();
            Password = (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmPassword) ?? String.Empty).ToString();
        }

        public async Task CacheTrackScrobble(TrackScrobble scrobble)
        {
            GetCredentialsFromSettings();
            if (AreCredentialsSet())
            {
                await DatabaseManager.Current.CacheTrackScrobbleAsync("track.scrobble", scrobble.Artist, scrobble.Track, scrobble.Timestamp);
            }
        }

        public async Task CacheTrackLove(string artist, string track, int rating)
        {
            if (AreCredentialsSet())
            {
                await DatabaseManager.Current.CacheTrackLoveAsync("track.love", artist, track);
            }
        }

        public async Task CacheTrackUnlove(string artist, string track, int rating)
        {
            if (AreCredentialsSet())
            {
                await DatabaseManager.Current.CacheTrackLoveAsync("track.unlove", artist, track);
            }
        }

        public async Task RateSong(string artist, string track, int rating)
        {
            bool rate = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmRateSongs);
            if (rate)
            {
                GetCredentialsFromSettings();
                if (AreCredentialsSet())
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
