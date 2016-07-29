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

namespace NextPlayerUWPDataLayer.OneDrive
{
    public sealed class OneDriveManager
    {
        private static readonly OneDriveManager instance = new OneDriveManager();

        static OneDriveManager() { }

        public static OneDriveManager Instance
        {
            get
            {
                return instance;
            }
        }

        private OneDriveManager()
        {
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
            try
            {
                oneDriveClient = await OneDriveClient.GetSilentlyAuthenticatedMicrosoftAccountClient(AppConstants.OneDriveAppId, "", scopes, refreshToken);
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
            return isLoggedIn;
        }

        public async Task Logout()
        {
            await oneDriveClient.SignOutAsync();
            SaveToken(null);
        }

        private static void SaveToken(string refreshToken)
        {
            ApplicationSettingsHelper.SaveSettingsValue(tokenName, refreshToken);
        }

        private static string GetSavedToken()
        {
            return ApplicationSettingsHelper.ReadSettingsValue(tokenName) as string;
        }

        #endregion

        public async Task<string> GetMusicFolderId()
        {
            try
            {
                var rootChildrens = await oneDriveClient.Drive.Root.Children.Request().GetAsync();
                return rootChildrens.FirstOrDefault(i => i.SpecialFolder.Name.Equals("music"))?.Id ?? "";
            }
            catch (OneDriveException ex)
            {
                return "";
            }
        }

        private Dictionary<string, IChildrenCollectionPage> cache = new Dictionary<string, IChildrenCollectionPage>();

        public async Task<List<SongItem>> GetSongItemsFromItem(string id)
        {
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

        public async Task<List<OneDriveFolder>> GetFoldersFromItem(string id)
        {
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
                    folders.Add(new OneDriveFolder(item.Name, "", item.Folder.ChildCount ?? 0, item.Id));
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
