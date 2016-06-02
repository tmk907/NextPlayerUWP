using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;

namespace NextPlayerUWPDataLayer.Services
{
    public enum StatusCode
    {
        Success,
        ReAuth,
        Cache,
        TryAgain,   
        Nothing
    }

    public class TrackScrobble
    {
        public string Artist { get; set; }
        public string Track { get; set; }
        public string Timestamp { get; set; }
    }

    public sealed class LastFmManager
    {
        private const string ApiKey = AppConstants.LastFmApiKey;
        private const string ApiSecret = AppConstants.LastFmApiSecret;

        private const string RootUrl = "http://ws.audioscrobbler.com/2.0/";
        private const string RootAuth = "https://ws.audioscrobbler.com/2.0/";

        private string Username = "";
        private string Password = "";

        private string SessionKey = "";

        private static readonly LastFmManager current = new LastFmManager();

        //// Explicit static constructor to tell C# compiler
        //// not to mark type as beforefieldinit
        //static LastFmManager()
        //{
        //}

        //public static LastFmManager Current
        //{
        //    get
        //    {
        //        return current;
        //    }
        //}
        
        public LastFmManager()
        {
            Username = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLogin) ?? String.Empty).ToString();
            Password = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmPassword) ?? String.Empty).ToString();
            SessionKey = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmSessionKey) ?? String.Empty).ToString();
        }

        public async Task<bool> Login(string login, string password)
        {
            Username = login;
            Password = password;
            await SetMobileSession();
            return IsSessionOn();
        }

        public void Logout()
        {
            Username = "";
            Password = "";
            SessionKey = "";
        }

        private async Task SetSession()
        {
            if (!IsSessionOn())
            {
                await SetMobileSession();
            }
        }

        public async Task<StatusCode> SetMobileSession()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("username", Username);
            data.Add("password", Password);
            data.Add("method", "auth.getMobileSession");
            data.Add("api_key", ApiKey);
            string signature = GetSignature(data);
            data.Add("api_sig", signature);

            string response = await SendMessage(data, true);

            if (IsStatusOK(response))
            {
                int i1 = response.IndexOf("<key>") + "<key>".Length;
                int i2 = response.IndexOf("</key>");
                SessionKey = response.Substring(i1, i2 - i1);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSessionKey, SessionKey);
                return StatusCode.Success;
            }
            else
            {
                var code = ParseError(response);
                return code;
            }
        }

        private async Task<string> SendMessage(Dictionary<string, string> data, bool isHttps)
        {
            string response = "";
            string host;
            if (isHttps)
            {
                host = RootAuth;
            }
            else
            {
                host = RootUrl;
            }
            using (HttpClient httpClient = new HttpClient())
            {
                using (var content = new FormUrlEncodedContent(data))
                {
                    try
                    {
                        using (var result = await httpClient.PostAsync(host, content))
                        {
                            response = await result.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SaveLastFm("LastFmManager SendMessage" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                }
            }
            return response;
        }


        private bool IsSessionOn()
        {
            return SessionKey != "";
        }

        private bool AreCredentialsSet()
        {
            return Username != "" && Password != "";
        }

        private bool IsStatusOK(string response)
        {
            return response.Contains("<lfm status=\"ok\">");
        }


        private string GetSignature(Dictionary<string,string> parameters)
        {
            StringBuilder builder = new StringBuilder();
            var ordered = parameters.OrderBy(p=>p.Key, StringComparer.Ordinal);

            foreach(var item in ordered)
            {
                builder.Append(item.Key).Append(item.Value);
            }
            builder.Append(ApiSecret);

            HashAlgorithmProvider md5hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer.ConvertStringToBinary(builder.ToString(), BinaryStringEncoding.Utf8);
            IBuffer buffHash = md5hasher.HashData(buffer);

            return CryptographicBuffer.EncodeToHexString(buffHash);
        }

        private void AddAuth(ref Dictionary<string, string> msg)
        {
            msg.Add("api_key", ApiKey);
            msg.Add("sk", SessionKey);
            string signature = GetSignature(msg);
            msg.Add("api_sig", signature);
        }


        private StatusCode ParseError(string response)
        {
            StatusCode statusCode = StatusCode.Nothing;
            if (IsStatusOK(response))
            {
                statusCode = StatusCode.Success;
            }
            else if (response.Contains("<error code="))
            {
                Logger.SaveLastFm(response);
                int i1 = response.IndexOf("code=\"") + "code=\"".Length;
                int i2 = response.IndexOf("\"", i1);
                int code;
                if (Int32.TryParse(response.Substring(i1, i2 - i1), out code))
                {
                    switch (code)
                    {
                        case 4:
                            statusCode = StatusCode.ReAuth;
                            break;
                        case 9:
                            statusCode = StatusCode.ReAuth;
                            break;
                        case 10:
                            statusCode = StatusCode.Cache;
                            break;
                        case 11:
                            statusCode = StatusCode.Cache;
                            break;
                        case 14:
                            statusCode = StatusCode.ReAuth;
                            break;
                        case 16:
                            statusCode = StatusCode.Cache;
                            break;
                        case 17:
                            statusCode = StatusCode.ReAuth;
                            break;
                        case 26:
                            statusCode = StatusCode.Cache;
                            break;
                        case 29:
                            statusCode = StatusCode.Cache;
                            break;
                        default:
                            break;
                    }
                }
                else
                {

                }
            }
            else
            {
                
            }
            return statusCode;
        }

        private async Task HadleError(StatusCode code, string function, object data)
        {
            if (code == StatusCode.ReAuth)
            {
                SessionKey = "";
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSessionKey, "");
                await SetMobileSession();
            }
            else if (code == StatusCode.Cache || code == StatusCode.Nothing)
            {
                string info = "";
                switch (function)
                {
                    case "track.scrobble":
                        List<TrackScrobble> list = (List<TrackScrobble>)data;
                        //foreach(var item in list)
                        //{
                        //    //await DatabaseManager.Current.SaveAsync(function, item.Artist, item.Track, item.Timestamp);
                        //}
                        info = list[0].Artist + " " + list[0].Track;
                        break;
                    case "track.love":
                        Tuple<string, string> t = (Tuple<string, string>)data;
                        //await DatabaseManager.Current.SaveAsync(function, t.Item1, t.Item2);
                        info = t.Item1 + "" + t.Item2;
                        break;
                    case "track.unlove":
                        Tuple<string, string> t2 = (Tuple<string, string>)data;
                        //await DatabaseManager.Current.SaveAsync(function, t2.Item1, t2.Item2);
                        info = t2.Item1 + "" + t2.Item2;
                        break;
                    case "track.nowplaying":
                        info = "nowplaying";
                        break;
                }
                Logger.SaveLastFm("Error Last.fm " + function + Environment.NewLine + info);
            }
        }


        private async Task<StatusCode> TrackScroblle(List<TrackScrobble> data)
        {
            if (!AreCredentialsSet()) return StatusCode.ReAuth;
            Dictionary<string, string> msg = new Dictionary<string, string>();
            if (data.Count == 1)
            {
                msg.Add("artist", data[0].Artist);
                msg.Add("track", data[0].Track);
                msg.Add("timestamp", data[0].Timestamp);
            }
            else
            {
                for(int i = 0; i < data.Count; i++)
                {
                    msg.Add("artist[" + i + "]", data[i].Artist);
                    msg.Add("track[" + i + "]", data[i].Track);
                    msg.Add("timestamp[" + i + "]", data[i].Timestamp);
                }
            }
            
            msg.Add("method", "track.scrobble");
            AddAuth(ref msg);

            string response = await SendMessage(msg, false);
            var errorCode = ParseError(response);
            return errorCode;
        }

        private async Task<StatusCode> TrackLove(string artist, string track)
        {
            if (!AreCredentialsSet()) return StatusCode.ReAuth;
            Dictionary<string, string> msg = new Dictionary<string, string>
            {
                {"artist", artist},
                {"track", track},
                {"method", "track.love"}
            };
            AddAuth(ref msg);

            string response = await SendMessage(msg, false);
            var errorCode = ParseError(response);
            return errorCode;
        }

        private async Task<StatusCode> TrackUnlove(string artist, string track)
        {
            if (!AreCredentialsSet()) return StatusCode.ReAuth;
            Dictionary<string, string> msg = new Dictionary<string, string>
            {
                {"artist", artist},
                {"track", track},
                {"method", "track.unlove"}
            };
            AddAuth(ref msg);

            string response = await SendMessage(msg, false);
            var errorCode = ParseError(response);
            return errorCode;
        }

        public async Task<StatusCode> TrackUpdateNowPlaying(string artist, string track)
        {
            if (!AreCredentialsSet()) return StatusCode.ReAuth;
            Dictionary<string, string> msg = new Dictionary<string, string>
            {
                {"artist", artist},
                {"track", track},
                {"method", "track.updateNowPlaying"}
            };
            AddAuth(ref msg);

            string response = await SendMessage(msg, false);
            var errorCode = ParseError(response);
            return errorCode;
        }


        public async Task SendCachedScrobbles()
        {
            if (!AreCredentialsSet()) return;

            var savedScrobbles = await DatabaseManager.Current.GetCachedScrobblesAsync();
            if (savedScrobbles.Count == 0) return;

            List<TrackScrobble> tracks = new List<TrackScrobble>();
            StatusCode code = StatusCode.Success;

            foreach (var scrobble in savedScrobbles.Where(s=>s.Function == "track.scrobble"))
            {
                tracks.Add(new TrackScrobble() { Artist = scrobble.Artist, Timestamp = scrobble.Timestamp, Track = scrobble.Track });
            }
            while (tracks.Count > 50 && code == StatusCode.Success)
            {
                code = await TrackScroblle(tracks.Take(50).ToList());
                if (code == StatusCode.Success)
                {
                    tracks.RemoveRange(0, 50);
                }
                else
                {
                    await HadleError(code, "track.scrobble", tracks.Take(1).ToList());
                }
                
            }
            if (tracks.Count > 0)
            {
                code = await TrackScroblle(tracks);
                if (code == StatusCode.Success)
                {
                    tracks.Clear();
                }
                else
                {
                    await HadleError(code, "track.scrobble", tracks.Take(1).ToList());
                }
            }
            await DatabaseManager.Current.DeleteCachedScrobblesTrack();
            if (tracks.Count > 0)
            {
                await DatabaseManager.Current.CacheTrackScrobblesAsync(tracks);
            }

            foreach (var scrobble in savedScrobbles.Where(s => (s.Function != "track.scrobble")))
            {
                switch (scrobble.Function)
                {
                    case "track.love":
                        code = await TrackLove(scrobble.Artist, scrobble.Track);
                        if (code == StatusCode.Success)
                        {
                            DatabaseManager.Current.DeleteCachedScrobble(scrobble.id);
                        }
                        else
                        {
                            await DatabaseManager.Current.CacheTrackLoveAsync(scrobble.Function, scrobble.Artist, scrobble.Track);
                        }
                        break;
                    case "track.unlove":
                        code = await TrackUnlove(scrobble.Artist, scrobble.Track);
                        if (code == StatusCode.Success)
                        {
                            DatabaseManager.Current.DeleteCachedScrobble(scrobble.id);
                        }
                        else
                        {
                            await DatabaseManager.Current.CacheTrackLoveAsync(scrobble.Function, scrobble.Artist, scrobble.Track);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        

        public async Task CacheTrackScrobble(TrackScrobble scrobble)
        {
            if (AreCredentialsSet())
            {
                await DatabaseManager.Current.CacheTrackScrobbleAsync("track.scrobble", scrobble.Artist, scrobble.Track, scrobble.Timestamp);
            }
        }

        public async Task CacheTrackLove(string artist, string track)
        {
            if (AreCredentialsSet())
            {
                await DatabaseManager.Current.CacheTrackLoveAsync("track.love", artist, track);
            }
        }

        public async Task CacheTrackUnlove(string artist, string track)
        {
            if (AreCredentialsSet())
            {
                await DatabaseManager.Current.CacheTrackLoveAsync("track.unlove", artist, track);
            }
        }

        public async Task TrackGetInfo(string title, string artist, bool autocorrect = true)
        {
            Dictionary<string, string> msg = new Dictionary<string, string>();
            msg.Add("method", "track.getInfo");
            msg.Add("api_key", ApiKey);
            msg.Add("artist", artist);
            msg.Add("track", title);
            msg.Add("autocorrect", (autocorrect ? 1 : 0).ToString());
            
            string response = await SendMessage(msg, false);

            if (!IsStatusOK(response))
            {
                //await HadleError(ParseError(response), "track.scrobble", data);
            }
        }
    }
}
