using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.CloudStorage.OneDrive
{
    public class OneDriveService : ICloudStorageService
    {
        public OneDriveService()
        {
            Debug.WriteLine("OneDriveService()");
            msaAuthenticationProvider = new MsaAuthenticationProvider(AppConstants.OneDriveAppId, "https://login.live.com/oauth20_desktop.srf", scopes);
            oneDriveClient = new OneDriveClient("https://api.onedrive.com/v1.0", msaAuthenticationProvider);

        }

        public OneDriveService(string UserId)
        {
            Debug.WriteLine("OneDriveService({0})", UserId);
            userId = UserId;
            msaAuthenticationProvider = new MsaAuthenticationProvider(AppConstants.OneDriveAppId, "https://login.live.com/oauth20_desktop.srf", scopes);
            oneDriveClient = new OneDriveClient("https://api.onedrive.com/v1.0", msaAuthenticationProvider);
        }

        private IOneDriveClient oneDriveClient { get; set; }
        private MsaAuthenticationProvider msaAuthenticationProvider { get; set; }
        private string userId;

        #region Authentication

        private readonly string[] scopes = new string[] { "onedrive.readonly", "wl.signin", "wl.offline_access" };
        private string refreshToken;

        public bool IsAuthenticated
        {
            get { return (((MsaAuthenticationProvider)oneDriveClient.AuthenticationProvider).IsAuthenticated); }
        }

        public async Task<bool> LoginSilently()
        {
            Debug.WriteLine("OneDriveService LoginSilently()");
            bool isLoggedIn = false;
            refreshToken = await GetSavedToken();
            if (IsAuthenticated)
            {
                return true;
            }
            if (!String.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    //    oneDriveClient = await OneDriveClient.GetSilentlyAuthenticatedMicrosoftAccountClient(AppConstants.OneDriveAppId, "", scopes, refreshToken);

                    AccountSession session = new AccountSession();
                    session.ClientId = AppConstants.OneDriveAppId;
                    session.RefreshToken = refreshToken;
                    //var msaAuthenticationProvider = new MsaAuthenticationProvider(AppConstants.OneDriveAppId, "https://login.live.com/oauth20_desktop.srf", scopes);
                    //oneDriveClient = new OneDriveClient(msaAuthenticationProvider);
                    msaAuthenticationProvider.CurrentAccountSession = session;
                    await msaAuthenticationProvider.AuthenticateUserAsync();
                    isLoggedIn = true;
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    if (!ex.IsMatch(OneDriveErrorCode.ServiceNotAvailable.ToString()) &&
                        !ex.IsMatch(OneDriveErrorCode.Timeout.ToString()))
                    {
                        refreshToken = null;
                        //oneDriveClient = OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(AppConstants.OneDriveAppId, scopes);
                    }
                }
            }
            return isLoggedIn;
        }

        public async Task<bool> Login()
        {
            Debug.WriteLine("OneDriveService Login()");
            bool isLoggedIn = false;
            if (!IsAuthenticated)
            {
                try
                {
                    await msaAuthenticationProvider.AuthenticateUserAsync();
                    if (refreshToken == null)
                    {
                        refreshToken = (((MsaAuthenticationProvider)oneDriveClient.AuthenticationProvider).CurrentAccountSession).RefreshToken;
                    }
                    else
                    {
                        if (refreshToken != (((MsaAuthenticationProvider)oneDriveClient.AuthenticationProvider).CurrentAccountSession).RefreshToken)
                        {
                            refreshToken = (((MsaAuthenticationProvider)oneDriveClient.AuthenticationProvider).CurrentAccountSession).RefreshToken;
                        }
                    }
                    isLoggedIn = true;
                    if (userId == null)
                    {
                        userId = ((MsaAuthenticationProvider)oneDriveClient.AuthenticationProvider).CurrentAccountSession.UserId;
                        string username = await GetUsername();
                        if (!CloudAccounts.Instance.Exists(userId, CloudStorageType.OneDrive))
                        {
                            await CloudAccounts.Instance.AddAccount(userId, CloudStorageType.OneDrive, username);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    await SaveToken(refreshToken);
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    if (!ex.IsMatch(OneDriveErrorCode.ServiceNotAvailable.ToString()) &&
                        !ex.IsMatch(OneDriveErrorCode.Timeout.ToString()))
                    {
                        refreshToken = null;
                        //oneDriveClient = OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(AppConstants.OneDriveAppId, scopes);
                    }
                }
            }
            else
            {
                isLoggedIn = true;
            }
            return isLoggedIn;
        }

        public bool Check(string userId, CloudStorageType type)
        {
            return (type == CloudStorageType.OneDrive) && userId == this.userId;
        }

        public async Task Logout()
        {
            Debug.WriteLine("OneDriveService Logout()");
            await msaAuthenticationProvider.SignOutAsync();
            await CloudAccounts.Instance.DeleteAccount(userId, CloudStorageType.OneDrive);
        }

        private async Task SaveToken(string refreshToken)
        {
            Debug.WriteLine("OneDriveService SaveToken()");
            await DatabaseManager.Current.SaveCloudAccountTokenAsync(userId, refreshToken);
        }

        private async Task<string> GetSavedToken()
        {
            Debug.WriteLine("OneDriveService GetSavedToken()");
            return await DatabaseManager.Current.GetCloudAccountTokenAsync(userId);
        }

        public async Task<CloudAccount> GetAccountInfo()
        {
            if (userId == null) return null;
            return CloudAccounts.Instance.GetAccount(userId);
        }

        private async Task<string> GetUsername()
        {
            var accessToken = ((MsaAuthenticationProvider)oneDriveClient.AuthenticationProvider).CurrentAccountSession.AccessToken;
            string username = "";
            var uri = new Uri($"https://apis.live.net/v5.0/me?access_token={accessToken}");
            var httpClient = new System.Net.Http.HttpClient();
            try
            {
                var result = await httpClient.GetAsync(uri);
                string jsonUserInfo = await result.Content.ReadAsStringAsync();
                if (jsonUserInfo != null)
                {
                    var json = Newtonsoft.Json.Linq.JObject.Parse(jsonUserInfo);
                    username = json["name"].ToString();
                }
            }
            catch (Exception ex)
            {

            }
            return username;
        }

        #endregion

        private string musicFolderId;
        public async Task<bool> IsMusicFolderId(string id)
        {
            if (musicFolderId == null)
            {
                var f = await GetMusicFolder();
                if (f == null) return false;
                musicFolderId = f.Id;
            }
            return musicFolderId == id;
        }
        private Dictionary<string, IItemChildrenCollectionPage> cache = new Dictionary<string, IItemChildrenCollectionPage>();
        private Dictionary<string, CloudFolder> cachedFolders = new Dictionary<string, CloudFolder>();

        public void ClearCache()
        {
            Debug.WriteLine("OneDriveService ClearCache()");
            cache = new Dictionary<string, IItemChildrenCollectionPage>();
            cachedFolders = new Dictionary<string, CloudFolder>();
            musicFolderId = null;
        }

        public async Task<string> GetRootFolderId()
        {
            var f = await GetMusicFolder();
            return f?.Id;
        }

        private async Task<CloudFolder> GetMusicFolder()
        {
            Debug.WriteLine("OneDriveService GetMusicFolder()");
            if (!IsAuthenticated) return null;
            try
            {
                if (musicFolderId != null && cachedFolders.ContainsKey(musicFolderId))
                {
                    return cachedFolders[musicFolderId];
                }
                var rootChildrens = await oneDriveClient.Drive.Root.Children.Request().GetAsync();
                var item = rootChildrens.FirstOrDefault(i => i.SpecialFolder.Name.Equals("music"));
                musicFolderId = item.Id;
                CloudFolder folder = new CloudFolder("OneDrive Music", "OneDrive Music", item.Folder.ChildCount ?? 0, item.Id, "", CloudStorageType.OneDrive, userId);
                cachedFolders.Add(musicFolderId, folder);
                return folder;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                return null;
            }
        }

        public async Task<List<SongItem>> GetSongItems(string folderId)
        {
            Debug.WriteLine("OneDriveService GetSongItems {0}", folderId);
            List<SongItem> songs = new List<SongItem>();
            if (!IsAuthenticated) return songs;

            IItemChildrenCollectionPage children;
            if (cache.ContainsKey(folderId))
            {
                children = cache[folderId];
            }
            else
            {
                try
                {
                    children = await oneDriveClient.Drive.Items[folderId].Children.Request().GetAsync();
                    cache.Add(folderId, children);
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    return songs;
                }
            }
            List<SongData> songData = new List<SongData>();
            foreach (var item in children)
            {
                if (item.Audio != null)
                {
                    songData.Add(CreateSongData(item, userId));
                }
            }
            songs = await DatabaseManager.Current.InsertCloudItems(songData, Enums.MusicSource.OneDrive);
            return songs;
        }

        public async Task<CloudFolder> GetFolder(string id)
        {
            Debug.WriteLine("OneDriveService GetFolder {0}", id);
            if (!IsAuthenticated) return null;
            if (cachedFolders.ContainsKey(id))
            {
                return cachedFolders[id];
            }
            try
            {
                var item = await oneDriveClient.Drive.Items[id].Request().GetAsync();
                if (item == null) return null;
                CloudFolder folder = new CloudFolder(item.Name, "", item.Folder.ChildCount ?? 0, item.Id, item.ParentReference.Id, CloudStorageType.OneDrive, userId);
                cachedFolders.Add(item.Id, folder);
                return folder;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                return null;
            }
        }

        public async Task<List<CloudFolder>> GetSubFolders(string folderId)
        {
            Debug.WriteLine("OneDriveService GetSubFoldersFromItem {0}", folderId);
            List<CloudFolder> folders = new List<CloudFolder>();
            if (!IsAuthenticated) return folders;

            IItemChildrenCollectionPage children;
            if (cache.ContainsKey(folderId))
            {
                children = cache[folderId];
            }
            else
            {
                try
                {
                    children = await oneDriveClient.Drive.Items[folderId].Children.Request().GetAsync();
                    cache.Add(folderId, children);
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    return folders;
                }
            }
            foreach (var item in children)
            {
                if (item.Folder != null)
                {
                    folders.Add(new CloudFolder(item.Name, "", item.Folder.ChildCount ?? 0, item.Id, folderId, CloudStorageType.OneDrive, userId));
                }
            }
            return folders;
        }


        public async Task<string> GetDownloadLink(string fileId)
        {
            var item = await oneDriveClient.Drive.Items[fileId].Request().GetAsync();
            return item.AdditionalData["@content.downloadUrl"].ToString();
        }

        private static SongData CreateSongData(Item item, string userId)
        {
            SongData song = new SongData();
            //song.AlbumArtPath = 
            song.Bitrate = (uint)(item?.Audio.Bitrate ?? 0);
            song.CloudUserId = userId;
            song.DateAdded = DateTime.Now;
            song.DateModified = (item.LastModifiedDateTime.HasValue) ? item.LastModifiedDateTime.Value.UtcDateTime : DateTime.UtcNow;
            song.Duration = TimeSpan.Zero;
            song.Filename = item.Name;
            song.FileSize = (ulong)(item.Size ?? 0);
            song.IsAvailable = 0;
            song.LastPlayed = DateTime.MinValue;
            song.MusicSourceType = (int)Enums.MusicSource.OneDrive;
            song.Path = item.Id;
            song.PlayCount = 0;
           
            song.Tag.Album = item?.Audio.Album ?? "";
            song.Tag.AlbumArtist = item?.Audio.AlbumArtist ?? "";
            song.Tag.Artists = item?.Audio.Artist ?? "";
            song.Tag.Comment = "";
            song.Tag.Composers = item?.Audio.Composers ?? "";
            song.Tag.Conductor = "";
            song.Tag.Disc = item?.Audio.Disc ?? 0;
            song.Tag.DiscCount = item?.Audio.DiscCount ?? 0;
            song.Tag.FirstArtist = item?.Audio.Artist ?? "";
            song.Tag.FirstComposer = item?.Audio.Composers ?? "";
            song.Tag.Genres = item?.Audio.Genre ?? "";
            song.Tag.Lyrics = "";
            song.Tag.Rating = 0;
            song.Tag.Title = String.IsNullOrEmpty(item?.Audio.Title) ? item.Name : item.Audio.Title;
            song.Tag.Track = item?.Audio.Track ?? 0;
            song.Tag.TrackCount = item?.Audio.TrackCount ?? 0;
            song.Tag.Year = item?.Audio.Year ?? 0;

            return song;
        }
    }
}
