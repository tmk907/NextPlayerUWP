using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Debug.WriteLine("OneDriveService() {0}", new object[] { UserId });
            userId = UserId;
            msaAuthenticationProvider = new MsaAuthenticationProvider(AppConstants.OneDriveAppId, "https://login.live.com/oauth20_desktop.srf", scopes, null, new OneDriveCredentialVault(vaultName, userId));
            oneDriveClient = new OneDriveClient("https://api.onedrive.com/v1.0", msaAuthenticationProvider);
        }

        private IOneDriveClient oneDriveClient { get; set; }
        private MsaAuthenticationProvider msaAuthenticationProvider { get; set; }
        private string userId;

        #region Authentication

        private readonly string vaultName = "Next-Player-OneDrive";
        private readonly string[] scopes = new string[] { "onedrive.readonly", "wl.signin", "wl.offline_access" };

        public bool IsAuthenticated
        {
            get { return msaAuthenticationProvider.IsAuthenticated; }
        }

        public async Task<bool> LoginSilently()
        {
            Debug.WriteLine("OneDriveService LoginSilently()");
            bool isLoggedIn = false;
            if (IsAuthenticated)
            {
                isLoggedIn = true;
            }
            else
            {
                try
                {
                    await msaAuthenticationProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
                    isLoggedIn = true;
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    if (!ex.IsMatch(OneDriveErrorCode.ServiceNotAvailable.ToString()) &&
                        !ex.IsMatch(OneDriveErrorCode.Timeout.ToString()))
                    {
                        
                    }
                }
            }
            return isLoggedIn;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// return true if NEW user is logged in
        /// return false if user failed to log in or user account already exists
        /// </returns>
        public async Task<bool> Login()
        {
            Debug.WriteLine("OneDriveService Login()");
            bool isLoggedIn = false;
            if (IsAuthenticated)
            {
                isLoggedIn = true;
            }
            else
            {
                try
                {
                    await msaAuthenticationProvider.AuthenticateUserAsync();
                    var currentAccountSession = msaAuthenticationProvider.CurrentAccountSession;
                    isLoggedIn = true;
                    if (userId == null)
                    {
                        userId = currentAccountSession.UserId;
                        var vault = new OneDriveCredentialVault(vaultName, userId);
                        vault.AddCredentialCacheToVault(msaAuthenticationProvider.CredentialCache);
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
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    if (!ex.IsMatch(OneDriveErrorCode.ServiceNotAvailable.ToString()) &&
                        !ex.IsMatch(OneDriveErrorCode.Timeout.ToString()))
                    {
                    }
                }
            }
            return isLoggedIn;
        }

        public async Task<bool> Login(string login, string password)
        {
            return false;
        }

        public bool Check(string userId, CloudStorageType type)
        {
            return (type == CloudStorageType.OneDrive) && userId == this.userId;
        }

        public async Task Logout()
        {
            Debug.WriteLine("OneDriveService Logout()");
            if (IsAuthenticated) await msaAuthenticationProvider.SignOutAsync();
            await CloudAccounts.Instance.DeleteAccount(userId, CloudStorageType.OneDrive);
        }

        public async Task<CloudAccount> GetAccountInfo()
        {
            if (userId == null) return null;
            return CloudAccounts.Instance.GetAccount(userId, CloudStorageType.OneDrive);
        }

        private async Task<string> GetUsername()
        {
            var accessToken = ((MsaAuthenticationProvider)oneDriveClient.AuthenticationProvider).CurrentAccountSession.AccessToken;
            string username = "";
            var uri = new Uri($"https://apis.live.net/v5.0/me?access_token={accessToken}");
            try
            {
                string jsonUserInfo = await ClientHttp.DownloadAsync(uri);
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
        private ConcurrentDictionary<string, Task<IList<Item>>> cache = new ConcurrentDictionary<string, Task<IList<Item>>>();
        private ConcurrentDictionary<string, Task<CloudFolder>> cachedFolders = new ConcurrentDictionary<string, Task<CloudFolder>>();

        public void ClearCache()
        {
            Debug.WriteLine("OneDriveService ClearCache()");
            cache = new ConcurrentDictionary<string, Task<IList<Item>>>();
            cachedFolders = new ConcurrentDictionary<string, Task<CloudFolder>>();
            musicFolderId = null;
        }

        public async Task<string> GetRootFolderId()
        {
            Debug.WriteLine("OneDriveService GetRootFolderId");
            await LoginSilently();
            if (!IsAuthenticated) return null;
            if (musicFolderId == null)
            {
                var rootChildrens = await oneDriveClient.Drive.Root.Children.Request().GetAsync();
                var item = rootChildrens.FirstOrDefault(i => (i?.SpecialFolder?.Name ?? "").Equals("music"));
                if (item == null)
                {
                    Diagnostics.Logger2.Current.WriteMessage($"OneDrive GetRootFolderId item == null, rootChildrens.Count = {rootChildrens.Count}", Diagnostics.Logger2.Level.WarningError);
                }
                musicFolderId = item?.Id;
            }
            if (musicFolderId == null) return null;
            var rootTask = cachedFolders.GetOrAdd(musicFolderId, GetRootMusicFolder);
            var f = await rootTask;
            return f?.Id;
        }

        private async Task<CloudFolder> GetRootMusicFolder(string musicFolderId)
        {
            Debug.WriteLine("OneDriveService GetRootMusicFolder {0}", new object[] { musicFolderId });
            await LoginSilently();
            if (!IsAuthenticated) return null;
            try
            {
                var rootChildrens = await oneDriveClient.Drive.Root.Children.Request().GetAsync();
                var item = rootChildrens.FirstOrDefault(i => (i?.SpecialFolder?.Name ?? "").Equals("music"));
                if (item == null)
                {
                    Diagnostics.Logger2.Current.WriteMessage($"OneDrive GetRootMusicFolder item == null, rootChildrens.Count = {rootChildrens.Count}", Diagnostics.Logger2.Level.WarningError);
                    return null;
                }
                CloudFolder folder = new CloudFolder(item.Name, item.ParentReference.Path, item.Folder.ChildCount ?? 0, item.Id, item.ParentReference.Id, CloudStorageType.OneDrive, userId);
                return folder;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                return null;
            }
        }

        
        public Task<CloudFolder> GetFolder(string folderId)
        {
            Debug.WriteLine("OneDriveService GetFolder {0}", new object[] { folderId });
            return cachedFolders.GetOrAdd(folderId, GetFolderInternalAsync);
        }

        public async Task<List<CloudFolder>> GetSubFolders(string folderId)
        {
            Debug.WriteLine("OneDriveService GetSubFolders {0}", new object[] { folderId });

            List<CloudFolder> folders = new List<CloudFolder>();
            if (String.IsNullOrEmpty(folderId)) return folders;

            var contentTask = cache.GetOrAdd(folderId, GetFolderContentInternalAsync);
            var content = await contentTask;

            if (content == null)
            {
                ClearCache();
                return folders;
            }
            foreach (var item in content)
            {
                if (item.Folder != null)
                {
                    folders.Add(new CloudFolder(item.Name, item.ParentReference.Path, item.Folder.ChildCount ?? 0, item.Id, folderId, CloudStorageType.OneDrive, userId));
                }
            }
            return folders;
        }

        public async Task<List<SongItem>> GetSongItems(string folderId)
        {
            Debug.WriteLine("OneDriveService GetSongItems {0}", new object[] { folderId });

            List<SongItem> songs = new List<SongItem>();
            List<SongData> songData = new List<SongData>();

            if (String.IsNullOrEmpty(folderId)) return songs;

            var contentTask = cache.GetOrAdd(folderId, GetFolderContentInternalAsync);
            var folderTask = cachedFolders.GetOrAdd(folderId, GetFolderInternalAsync);

            var content = await contentTask;
            var folder = await folderTask;

            if (content == null)
            {
                ClearCache();
                return songs;
            }

            foreach (var item in content)
            {
                if (item.Audio != null)
                {
                    songData.Add(CreateSongData(item, userId, folder));
                }
            }
            songs = await DatabaseManager.Current.InsertCloudItems(songData, Enums.MusicSource.OneDrive);
            return songs;
        }

        private async Task<CloudFolder> GetFolderInternalAsync(string folderId)
        {
            Debug.WriteLine("OneDrive GetFolderInternalAsync() {0}", new object[] { folderId });
            await LoginSilently();
            if (!IsAuthenticated || String.IsNullOrEmpty(folderId)) return null;
            try
            {
                var item = await oneDriveClient.Drive.Items[folderId].Request().GetAsync();
                if (item == null) return null;
                CloudFolder folder = new CloudFolder(item.Name, item.ParentReference.Path, item.Folder.ChildCount ?? 0, item.Id, item.ParentReference.Id, CloudStorageType.OneDrive, userId);
                return folder;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {

            }
            catch (Exception ex)
            {

            }
            return null;
        }


        private async Task<IList<Item>> GetFolderContentInternalAsync(string folderId)
        {
            Debug.WriteLine("OneDrive GetFolderContentInternalAsync() {0}", new object[] { folderId });
            await LoginSilently();
            if (!IsAuthenticated) return null;
            try
            {
                var children = await oneDriveClient.Drive.Items[folderId].Children.Request().GetAsync();
                var allItems = children.CurrentPage;
                while(children.NextPageRequest != null)
                {
                    children = await children.NextPageRequest.GetAsync();
                    foreach(var item in children.CurrentPage)
                    {
                        allItems.Add(item);
                    }
                }
                return allItems;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<string> GetDownloadLink(string fileId)
        {
            await LoginSilently();
            if (!IsAuthenticated) return null;
            var item = await oneDriveClient.Drive.Items[fileId].Request().GetAsync();
            return item.AdditionalData["@content.downloadUrl"].ToString();
        }

        private static SongData CreateSongData(Item item, string userId, CloudFolder folder)
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
            song.FolderName = folder.Folder;
            song.DirectoryPath = folder.Id;
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
