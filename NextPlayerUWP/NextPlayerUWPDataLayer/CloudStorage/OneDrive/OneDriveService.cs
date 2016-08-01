using Microsoft.OneDrive.Sdk;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.CloudStorage.OneDrive
{
    public delegate void AuthenticationChangeHandler(bool isAuthenticated);

    public sealed class OneDriveService
    {
        public static event AuthenticationChangeHandler AuthenticationChanged;

        public static void OnAuthenticationChanged(bool isAuthenticated)
        {
            AuthenticationChanged?.Invoke(isAuthenticated);
        }

        private static readonly OneDriveService instance = new OneDriveService();

        static OneDriveService() { }

        public static OneDriveService Instance
        {
            get
            {
                return instance;
            }
        }

        private OneDriveService()
        {
            Debug.WriteLine("OneDriveManager()");
            oneDriveClient = OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(AppConstants.OneDriveAppId, scopes);
            refreshToken = GetSavedToken();
            if (!String.IsNullOrEmpty(refreshToken))
            {
                LoginSilently();
            }
        }

        public IOneDriveClient oneDriveClient { get; set; }

        #region Authentication

        private readonly string[] scopes = new string[] { "onedrive.readonly", "wl.signin", "wl.offline_access" };
        private string refreshToken;
        private const string tokenName = "OneDriveToken";

        public bool IsAuthenticated
        {
            get { return (oneDriveClient.IsAuthenticated); }
        }

        private async Task LoginSilently()
        {
            Debug.WriteLine("OneDriveManager LoginSilently()");
            try
            {
                oneDriveClient = await OneDriveClient.GetSilentlyAuthenticatedMicrosoftAccountClient(AppConstants.OneDriveAppId, "", scopes, refreshToken);
                OnAuthenticationChanged(true);
            }
            catch (OneDriveException ex)
            {
                if (!ex.IsMatch(OneDriveErrorCode.ServiceNotAvailable.ToString()) &&
                    !ex.IsMatch(OneDriveErrorCode.Timeout.ToString()))
                {
                    refreshToken = null;
                    oneDriveClient = OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(AppConstants.OneDriveAppId, scopes);
                }
            }
        }

        public async Task<bool> Login()
        {
            Debug.WriteLine("OneDriveManager Login()");
            bool isLoggedIn = false;
            if (!oneDriveClient.IsAuthenticated)
            {
                try
                {
                    if (refreshToken == null)
                    {
                        var session = await oneDriveClient.AuthenticateAsync();
                        refreshToken = session.RefreshToken;
                        SaveToken(refreshToken);
                    }
                    else
                    {
                        await oneDriveClient.AuthenticateAsync();
                        if (refreshToken != oneDriveClient.AuthenticationProvider?.CurrentAccountSession?.RefreshToken)
                        {
                            refreshToken = oneDriveClient.AuthenticationProvider?.CurrentAccountSession?.RefreshToken;
                            SaveToken(refreshToken);
                        }
                    }
                    isLoggedIn = true;
                }
                catch (OneDriveException ex)
                {
                    if (!ex.IsMatch(OneDriveErrorCode.ServiceNotAvailable.ToString()) &&
                    !ex.IsMatch(OneDriveErrorCode.Timeout.ToString()))
                    {
                        refreshToken = null;
                        oneDriveClient = OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(AppConstants.OneDriveAppId, scopes);
                    }
                }
            }
            else
            {
                isLoggedIn = true;
            }
            OnAuthenticationChanged(isLoggedIn);
            return isLoggedIn;
        }

        public async Task Logout()
        {
            Debug.WriteLine("OneDriveManager Logout()");
            await oneDriveClient.SignOutAsync();
            OnAuthenticationChanged(false);
            SaveToken(null);
        }

        private static void SaveToken(string refreshToken)
        {
            Debug.WriteLine("OneDriveManager SaveToken()");
            ApplicationSettingsHelper.SaveSettingsValue(tokenName, refreshToken);
        }

        private static string GetSavedToken()
        {
            Debug.WriteLine("OneDriveManager GetSavedToken()");
            return ApplicationSettingsHelper.ReadSettingsValue(tokenName) as string;
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
        private Dictionary<string, IChildrenCollectionPage> cache = new Dictionary<string, IChildrenCollectionPage>();
        private Dictionary<string, OneDriveFolder> cachedFolders = new Dictionary<string, OneDriveFolder>();

        public void ClearCache()
        {
            Debug.WriteLine("OneDriveManager ClearCache()");
            cache = new Dictionary<string, IChildrenCollectionPage>();
            cachedFolders = new Dictionary<string, OneDriveFolder>();
            musicFolderId = null;
        }

        public async Task<OneDriveFolder> GetMusicFolder()
        {
            Debug.WriteLine("OneDriveManager GetMusicFolder()");
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
                OneDriveFolder folder = new OneDriveFolder("OneDrive Music", "OneDrive Music", item.Folder.ChildCount ?? 0, item.Id, "");
                cachedFolders.Add(musicFolderId, folder);
                return folder;
            }
            catch (OneDriveException ex)
            {
                return null;
            }
        }

        public async Task<List<SongItem>> GetSongItemsFromItem(string id)
        {
            Debug.WriteLine("OneDriveManager GetSongItemsFromItem({0})", id);
            List<SongItem> songs = new List<SongItem>();
            if (!IsAuthenticated) return songs;

            IChildrenCollectionPage children;
            if (cache.ContainsKey(id))
            {
                children = cache[id];
            }
            else
            {
                try
                {
                    children = await oneDriveClient.Drive.Items[id].Children.Request().GetAsync();
                    cache.Add(id, children);
                }
                catch (OneDriveException ex)
                {
                    return songs;
                }
            }
            foreach (var item in children)
            {
                if (item.Audio != null)
                {
                    songs.Add(OneDriveItemToSongItem(item));
                }
            }
            return songs;
        }

        public async Task<OneDriveFolder> GetFolder(string id)
        {
            Debug.WriteLine("OneDriveManager GetFolder({0})", id);
            if (!IsAuthenticated) return null;
            if (cachedFolders.ContainsKey(id))
            {
                return cachedFolders[id];
            }
            try
            {
                var item = await oneDriveClient.Drive.Items[id].Request().GetAsync();
                if (item == null) return null;
                OneDriveFolder folder = new OneDriveFolder(item.Name, "", item.Folder.ChildCount ?? 0, item.Id, item.ParentReference.Id);
                cachedFolders.Add(item.Id, folder);
                return folder;
            }
            catch (OneDriveException ex)
            {
                return null;
            }
        }

        public async Task<List<OneDriveFolder>> GetSubFoldersFromItem(string id)
        {
            Debug.WriteLine("OneDriveManager GetSubFoldersFromItem({0})", id);
            List<OneDriveFolder> folders = new List<OneDriveFolder>();
            if (!IsAuthenticated) return folders;

            IChildrenCollectionPage children;
            if (cache.ContainsKey(id))
            {
                children = cache[id];
            }
            else
            {
                try
                {
                    children = await oneDriveClient.Drive.Items[id].Children.Request().GetAsync();
                    cache.Add(id, children);
                }
                catch (OneDriveException ex)
                {
                    return folders;
                }
            }
            foreach (var item in children)
            {
                if (item.Folder != null)
                {
                    folders.Add(new OneDriveFolder(item.Name, "", item.Folder.ChildCount ?? 0, item.Id, id));
                }
            }
            return folders;
        }

        private static SongItem OneDriveItemToSongItem(Item item)
        {
            SongItem song = new SongItem();
            song.Album = item?.Audio.Album ?? "";
            song.AlbumArtist = item?.Audio.AlbumArtist ?? "";
            song.Artist = item?.Audio.Artist ?? "";
            song.Composer = item?.Audio.Composers ?? "";
            //song.CoverPath = item?.Audio. ?? "";
            //song.DateAdded = item.CreatedDateTime.Value.DateTime;
            song.Disc = item?.Audio.Disc ?? 0;
            song.Duration = (item?.Audio.Duration != null) ? TimeSpan.FromMilliseconds((double)item.Audio.Duration) : TimeSpan.Zero;
            song.Genres = item?.Audio.Genre ?? "";
            song.Path = item.AdditionalData["@content.downloadUrl"].ToString();
            song.SourceType = Enums.MusicSource.OneDrive;
            song.Title = String.IsNullOrEmpty(item?.Audio.Title) ? item.Name : item?.Audio.Title;
            song.TrackNumber = item?.Audio.Track ?? 0;
            song.Year = item?.Audio.Year ?? 0;
            song.GenerateID();
            return song;
        }

        
    }
}
