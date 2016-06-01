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
            Username = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLogin) ?? String.Empty).ToString();
            Password = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmPassword) ?? String.Empty).ToString();
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

        public void RefreshCredentials()
        {
            Username = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLogin) ?? String.Empty).ToString();
            Password = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmPassword) ?? String.Empty).ToString();
        }

        public async Task CacheTrackScrobble(TrackScrobble scrobble)
        {
            RefreshCredentials();
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
            bool rate = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmRateSongs);
            if (rate)
            {
                RefreshCredentials();
                if (AreCredentialsSet())
                {
                    int min = (int)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLove);
                    if (rating > min)
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
