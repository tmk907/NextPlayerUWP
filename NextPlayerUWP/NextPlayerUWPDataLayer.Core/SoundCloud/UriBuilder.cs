using NextPlayerUWPDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.SoundCloud
{
    public class UriBuilder
    {
        public const string BaseUrl = "https://api.soundcloud.com";

        public string GetUser(int id)
        {
            return $"{BaseUrl}/users/{id}?client_id={AppConstants.SoundCloudClientId}";
        }

        public string GetUserTracks(int id)
        {
            return $"{BaseUrl}/users/{id}/tracks?client_id={AppConstants.SoundCloudClientId}";
        }

        public string GetUserPlaylists(int id)
        {
            return $"{BaseUrl}/users/{id}/playlists?client_id={AppConstants.SoundCloudClientId}";
        }

        public string GetTrack(int id)
        {
            return $"{BaseUrl}/tracks/{id}?client_id={AppConstants.SoundCloudClientId}";
        }

        public string GetPlaylist(int id)
        {
            return $"{BaseUrl}/playlists/{id}?client_id={AppConstants.SoundCloudClientId}";
        }

        public string SearchUsers(string param)
        {
            return $"{BaseUrl}/users/?q={param}&client_id={AppConstants.SoundCloudClientId}";
        }

        public string SearchTracks(string param)
        {
            return $"{BaseUrl}/tracks/?q={param}&client_id={AppConstants.SoundCloudClientId}";
        }

        public string SearchPlaylists(string param)
        {
            return $"{BaseUrl}/playlists/?q={param}&client_id={AppConstants.SoundCloudClientId}";
        }
    }
}
