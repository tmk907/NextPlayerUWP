using Newtonsoft.Json;
using NextPlayerUWPDataLayer.SoundCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.SoundCloud
{
    public class SoundCloudClient
    {
        public JsonSerializerSettings JsonSettings { get; set; }

        private UriBuilder uriBuilder;

        public SoundCloudClient()
        {
            uriBuilder = new UriBuilder();
        }

        private async Task<string> DownloadAsync(string url)
        {
            String response;
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            try
            {
                Uri uri = new Uri(url);
                httpResponse = await httpClient.GetAsync(uri);
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                response = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            return response;
        }

        private async Task<T> DownloadJsonAsync<T>(string url)
        {
            String response = await DownloadAsync(url);
            return JsonConvert.DeserializeObject<T>(response, JsonSettings);
        }

        public async Task<Track> GetTrack(int id)
        {
            string uri = uriBuilder.GetTrack(id);
            Track track = await DownloadJsonAsync<Track>(uri);
            return track;
        }

        public async Task<Playlist> GetPlaylist(int id)
        {
            string uri = uriBuilder.GetPlaylist(id);
            Playlist playlist = await DownloadJsonAsync<Playlist>(uri);
            return playlist;
        }

        public async Task<List<Track>> SearchTracks(string query)
        {
            string uri = uriBuilder.SearchTracks(query);
            List<Track> tracks = await DownloadJsonAsync<List<Track>>(uri);
            return tracks;
        }

        public async Task<List<Playlist>> SearchPlaylists(string query)
        {
            string uri = uriBuilder.SearchPlaylists(query);
            List<Playlist> tracks = await DownloadJsonAsync<List<Playlist>>(uri);
            return tracks;
        }
    }
}
