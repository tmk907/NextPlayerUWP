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

namespace NextPlayerUWPDataLayer.CloudStorage.DropboxStorage
{
    public class DropboxService : ICloudStorageService
    {
        public static event AuthenticationChangeHandler AuthenticationChanged;

        public static void OnAuthenticationChanged(bool isAuthenticated)
        {
            AuthenticationChanged?.Invoke(isAuthenticated);
        }

        public DropboxService()
        {
            Debug.WriteLine("DropboxService()");
        }

        public DropboxService(string userId)
        {
            Debug.WriteLine("DropboxService({0})", userId);
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
                    await CloudAccounts.Instance.AddAccount(userId, CloudStorageType.Dropbox, username);
                    await SaveToken(refreshToken);
                }
            }
            OnAuthenticationChanged(isLoggedIn);
            return isLoggedIn;
        }

        public async Task Logout()
        {
            Debug.WriteLine("DropboxService Logout()");
            OnAuthenticationChanged(false);
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
            return CloudAccounts.Instance.GetAccount(userId);
        }

        public bool Check(string userId, CloudStorageType type)
        {
            return (type == CloudStorageType.Dropbox) && userId == this.userId;
        }

        public async Task<string> GetRootFolderId()
        {
            return "";
        }

        public async Task<CloudFolder> GetFolder(string id)
        {
            Debug.WriteLine("DropboxService GetFolder({0})", id);
            if (!IsAuthenticated) return null;
            //if (cachedFolders.ContainsKey(id))
            //{
            //    return cachedFolders[id];
            //}
            if (id == "")
            {
                return new CloudFolder("Dropbox", "", 0, "", null, MusicItemTypes.dropboxfolder);
            }
            try
            {
                var item = await dropboxClient.Files.GetMetadataAsync(id);
                if (item == null) return null;
                CloudFolder folder = new CloudFolder(item.Name, "", 0, item.PathLower, System.IO.Path.GetDirectoryName(item.PathLower), MusicItemTypes.dropboxfolder);
                //cachedFolders.Add(item.Id, folder);
                return folder;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<SongItem>> GetSongItems(string id)
        {
            List<SongItem> songs = new List<SongItem>();
            if (!IsAuthenticated) return songs;
            var args = new Dropbox.Api.Files.ListFolderArg("", false, true, false, false);
            var result = await dropboxClient.Files.ListFolderAsync(args);
            foreach (var item in result.Entries.Where(i=>i.IsFile))
            {
                var f = item.AsFile;
                SongItem s = new SongItem();
                s.SourceType = Enums.MusicSource.Dropbox;
                s.Title = f.Name;
                s.Path = f.PathLower;
                s.GenerateID();
                songs.Add(s);
            }
            return songs;
        }

        public async Task<List<CloudFolder>> GetSubFolders(string id)
        {
            List<CloudFolder> folders = new List<CloudFolder>();
            if (!IsAuthenticated) return folders;
            var args = new Dropbox.Api.Files.ListFolderArg("", false, true, false, false);
            var result = await dropboxClient.Files.ListFolderAsync(args);
            foreach (var item in result.Entries.Where(i => i.IsFolder))
            {
                var f = item.AsFolder;
                folders.Add(new CloudFolder(f.Name, f.PathLower, 0, f.Id, id, MusicItemTypes.dropboxfolder));
            }
            return folders;
        }

        //Expire in 4 hours
        public async Task<string> GetTemporaryLink(string path)
        {
            if (!IsAuthenticated) return null;
            var link = await dropboxClient.Files.GetTemporaryLinkAsync(path);
            return link.Link;
        }
    }
}
