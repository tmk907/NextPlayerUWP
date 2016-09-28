using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using System.Diagnostics;
using Dropbox.Api;
using NextPlayerUWPDataLayer.Constants;
using Windows.Security.Authentication.Web;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System.Collections.Concurrent;

namespace NextPlayerUWPDataLayer.CloudStorage.DropboxStorage
{
    public class DropboxService : ICloudStorageService
    {
        public DropboxService()
        {
            Debug.WriteLine("DropboxService()");
        }

        public DropboxService(string userId)
        {
            Debug.WriteLine("DropboxService() {0}", new object[] { userId });
            this.userId = userId;
        }

        private DropboxClient dropboxClient;
        private string refreshToken;
        private static readonly Uri redirectUri = new Uri("http://localhost/next-player/dropbox/authorize");
        private string userId;

        private async Task Authorize()
        {
            var authUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, AppConstants.DropboxAppKey, redirectUri);
            try
            {
                var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, authUri, redirectUri);
                ProcessResult(result);
            }
            catch(Exception ex)
            {

            }
        }

        private void ProcessResult(WebAuthenticationResult result)
        {
            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:                    
                    var response = DropboxOAuth2Helper.ParseTokenFragment(new Uri(result.ResponseData));
                        refreshToken = response.AccessToken;
                        dropboxClient = new DropboxClient(refreshToken);
                    break;
                case WebAuthenticationStatus.ErrorHttp:
                    throw new OAuthException(result.ResponseErrorDetail);

                case WebAuthenticationStatus.UserCancel:
                default:
                    throw new OAuthUserCancelledException();
            }
        }

        #region Exceptions
        /// <summary>
        /// An exception raised if there was an HTTP error in the authentication process. 
        /// </summary>
        public class OAuthException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OAuthException"/> class.
            /// </summary>
            /// <param name="statusCode">The HTTP status code.</param>
            public OAuthException(uint statusCode)
            {
                this.StatusCode = statusCode;
            }

            /// <summary>
            /// Gets the HTTP status code.
            /// </summary>
            public uint StatusCode { get; private set; }
        }

        /// <summary>
        /// An exception raised if the user cancelled the authentication.
        /// </summary>
        public class OAuthUserCancelledException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OAuthUserCancelledException"/> class.
            /// </summary>
            public OAuthUserCancelledException()
            {
            }
        }
        #endregion

        public bool IsAuthenticated
        {
            get
            {
                if (dropboxClient == null || refreshToken == null) return false;
                else return true;
            }
        }

        public async Task<bool> LoginSilently()
        {
            if (IsAuthenticated)
            {
                return true;
            }
            Debug.WriteLine("DropboxService LoginSilently()");
            refreshToken = await GetSavedToken();
            dropboxClient = new DropboxClient(refreshToken);
            return IsAuthenticated;
        }

        public async Task<bool> Login()
        {
            Debug.WriteLine("DropboxService Login()");

            bool isLoggedIn = false;
            if (IsAuthenticated)
            {
                dropboxClient = new DropboxClient(refreshToken);
                isLoggedIn = true;
            }
            else
            {
                await Authorize();
                isLoggedIn = IsAuthenticated;

                if (IsAuthenticated && userId == null)
                {
                    var account = await dropboxClient.Users.GetCurrentAccountAsync();
                    userId = account.AccountId;
                    string username = account.Name.DisplayName;
                    if (!CloudAccounts.Instance.Exists(userId, CloudStorageType.Dropbox))
                    {
                        await CloudAccounts.Instance.AddAccount(userId, CloudStorageType.Dropbox, username);
                    }
                    else
                    {
                        return false;
                    }
                    await SaveToken(refreshToken);
                }
            }
            return isLoggedIn;
        }

        public async Task<bool> Login(string login, string password)
        {
            return false;
        }

        public async Task Logout()
        {
            Debug.WriteLine("DropboxService Logout()");
            refreshToken = null;
            await dropboxClient.Auth.TokenRevokeAsync();
            await CloudAccounts.Instance.DeleteAccount(userId, CloudStorageType.Dropbox);
        }

        private async Task SaveToken(string refreshToken)
        {
            Debug.WriteLine("DropboxService SaveToken()");
            await DatabaseManager.Current.SaveCloudAccountTokenAsync(userId, refreshToken);
        }

        private async Task<string> GetSavedToken()
        {
            Debug.WriteLine("DropboxService GetSavedToken()");
            return await DatabaseManager.Current.GetCloudAccountTokenAsync(userId);
        }

        public async Task<CloudAccount> GetAccountInfo()
        {
            if (userId == null) return null;
            return CloudAccounts.Instance.GetAccount(userId, CloudStorageType.Dropbox);
        }

        public bool Check(string userId, CloudStorageType type)
        {
            return (type == CloudStorageType.Dropbox) && userId == this.userId;
        }


        private ConcurrentDictionary<string, Task<Dropbox.Api.Files.ListFolderResult>> cache = new ConcurrentDictionary<string, Task<Dropbox.Api.Files.ListFolderResult>>();
        private ConcurrentDictionary<string, Task<CloudFolder>> cachedFolders = new ConcurrentDictionary<string, Task<CloudFolder>>();

        public void ClearCache()
        {
            Debug.WriteLine("OneDriveService ClearCache()");
            cache = new ConcurrentDictionary<string, Task<Dropbox.Api.Files.ListFolderResult>>();
            cachedFolders = new ConcurrentDictionary<string, Task<CloudFolder>>();
        }

        public async Task<string> GetRootFolderId()
        {
            Debug.WriteLine("DropboxService GetRootFolderId()");
            var f = cachedFolders.GetOrAdd("", GetFolderInternalAsync);
            await f;
            return "";
        }

        public Task<CloudFolder> GetFolder(string path)
        {
            Debug.WriteLine("DropboxService GetFolder() {0}", new object[] { path });
            return cachedFolders.GetOrAdd(path, GetFolderInternalAsync);
        }

        public async Task<List<SongItem>> GetSongItems(string path)
        {
            Debug.WriteLine("DropboxService GetSongItems() {0}", new object[] { path });
            var contentTask = cache.GetOrAdd(path, GetFolderContentInternalAsync);
            var folderTask = cachedFolders.GetOrAdd(path, GetFolderInternalAsync);

            var content = await contentTask;
            var folder = await folderTask;

            List<SongItem> songs = new List<SongItem>();
            List<SongData> songData = new List<SongData>();
            if (content == null)
            {
                ClearCache();
                return songs;
            }
            foreach (var item in content.Entries.Where(i=>i.IsFile))
            {
                var itemAsFile = item.AsFile;
                if (itemAsFile.Name.ToLower().EndsWith(".mp3") || itemAsFile.Name.ToLower().EndsWith(".m4a"))
                {
                    songData.Add(CreateSongData(itemAsFile, userId, folder));
                }
            }

            songs = await DatabaseManager.Current.InsertCloudItems(songData, Enums.MusicSource.Dropbox);

            return songs;
        }

        public async Task<List<CloudFolder>> GetSubFolders(string path)
        {
            Debug.WriteLine("DropboxService GetSubFolders() {0}", new object[] { path });
            var contentTask = cache.GetOrAdd(path, GetFolderContentInternalAsync);
            var content = await contentTask;

            List<CloudFolder> folders = new List<CloudFolder>();
            if (content == null)
            {
                ClearCache();
                return folders;
            }

            foreach (var item in content.Entries.Where(i => i.IsFolder))
            {
                var f = item.AsFolder;
                folders.Add(new CloudFolder(f.Name, f.PathDisplay, 0, f.PathLower, (path == "") ? "" : System.IO.Path.GetDirectoryName(path), CloudStorageType.Dropbox, userId));
            }
            return folders;
        }

        private async Task<CloudFolder> GetFolderInternalAsync(string path)
        {
            Debug.WriteLine("DropboxService GetFolderInternalAsync() {0}", new object[] { path });
            await LoginSilently();
            if (!IsAuthenticated) return null;
            if (path == "")
            {
                return new CloudFolder("Dropbox", "", 0, "", null, CloudStorageType.Dropbox, userId);
            }
            try
            {
                var item = await dropboxClient.Files.GetMetadataAsync(path);
                if (item == null) return null;
                CloudFolder folder = new CloudFolder(item.Name, item.PathDisplay, 0, item.PathLower, (path == "") ? "" : System.IO.Path.GetDirectoryName(item.PathLower), CloudStorageType.Dropbox, userId);
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

        private async Task<Dropbox.Api.Files.ListFolderResult> GetFolderContentInternalAsync(string path)
        {
            Debug.WriteLine("DropboxService GetFolderContentInternalAsync() {0}", new object[] { path });
            await LoginSilently();
            if (!IsAuthenticated) return null;
            try
            {
                var args = new Dropbox.Api.Files.ListFolderArg(path, false, true, false, false);
                var result = await dropboxClient.Files.ListFolderAsync(args);
                return result;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                
            }
            catch(Exception ex)
            {

            }
            return null;
        }

        //Expire in 4 hours
        public async Task<string> GetDownloadLink(string path)
        {
            await LoginSilently();
            if (!IsAuthenticated) return null;
            var link = await dropboxClient.Files.GetTemporaryLinkAsync(path);
            return link.Link;
        }

        private static SongData CreateSongData(Dropbox.Api.Files.FileMetadata item, string userId, CloudFolder folder)
        {
            SongData song = new SongData();
            //song.AlbumArtPath = 
            song.Bitrate = 0;
            song.CloudUserId = userId;
            song.DateAdded = DateTime.Now;
            song.DateModified = item.ServerModified;
            song.Duration = TimeSpan.Zero;
            song.Filename = item.Name;
            song.FileSize = item.Size;
            song.IsAvailable = 0;
            song.LastPlayed = DateTime.MinValue;
            song.MusicSourceType = (int)Enums.MusicSource.Dropbox;
            song.Path = item.Id;
            song.FolderName = folder.Folder;
            song.DirectoryPath = folder.Id;
            song.PlayCount = 0;

            song.Tag.Album = "";
            song.Tag.AlbumArtist = "";
            song.Tag.Artists = "";
            song.Tag.Comment = "";
            song.Tag.Composers = "";
            song.Tag.Conductor = "";
            song.Tag.Disc = 0;
            song.Tag.DiscCount = 0;
            song.Tag.FirstArtist = "";
            song.Tag.FirstComposer = "";
            song.Tag.Genres = "";
            song.Tag.Lyrics = "";
            song.Tag.Rating = 0;
            song.Tag.Title = item.Name;
            song.Tag.Track = 0;
            song.Tag.TrackCount = 0;
            song.Tag.Year = 0;

            return song;
        }
    }
}
