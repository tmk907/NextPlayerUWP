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

namespace NextPlayerUWPDataLayer.CloudStorage.DropboxStorage
{
    public sealed class DropboxService// : ICloudStorageService
    {
        public static event AuthenticationChangeHandler AuthenticationChanged;

        public static void OnAuthenticationChanged(bool isAuthenticated)
        {
            AuthenticationChanged?.Invoke(isAuthenticated);
        }

        private static readonly DropboxService instance = new DropboxService();

        static DropboxService() { }

        public static DropboxService Instance
        {
            get
            {
                return instance;
            }
        }

        private DropboxService()
        {
            Debug.WriteLine("DropboxService()");
            if (AuthToken != null)
            {
                dropboxClient = new DropboxClient(AuthToken);
                OnAuthenticationChanged(true);
            }
        }

        private DropboxClient dropboxClient;
        private string authToken;
        private string AuthToken
        {
            get
            {
                if (authToken == null)
                {
                    authToken = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DropboxAuthToken) as string;
                }
                return authToken;
            }
            set
            {
                if (authToken != value)
                {
                    authToken = value;
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DropboxAuthToken, authToken);
                }
            }
        }
        private static readonly Uri redirectUri = new Uri("http://localhost/next-player/dropbox/authorize");

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
                        AuthToken = response.AccessToken;
                        dropboxClient = new DropboxClient(AuthToken);
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
                if (dropboxClient == null || AuthToken == null) return false;
                else return true;
            }
        }

        public async Task<bool> Login()
        {
            Debug.WriteLine("DropboxService Login()");

            bool isLoggedIn = false;
            if (IsAuthenticated)
            {
                dropboxClient = new DropboxClient(AuthToken);
                isLoggedIn = true;
            }
            else
            {
                await Authorize();
                isLoggedIn = IsAuthenticated;
            }
            OnAuthenticationChanged(isLoggedIn);
            return isLoggedIn;
        }

        public async Task Logout()
        {
            Debug.WriteLine("DropboxService Logout()");
            OnAuthenticationChanged(false);
            AuthToken = null;
            await dropboxClient.Auth.TokenRevokeAsync();
        }

        public async Task<CloudFolder> GetFolder(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<CloudFolder> GetRootFolder()
        {
            if (!IsAuthenticated) return null;
            return new CloudFolder("Dropbox", "", 0, "", null, MusicItemTypes.dropboxfolder);
        }

        public async Task<List<SongItem>> GetSongItemsFromItem(string id)
        {
            List<SongItem> songs = new List<SongItem>();
            if (!IsAuthenticated) return songs;
            var args = new Dropbox.Api.Files.ListFolderArg("", false, true, false, false);
            var result = await dropboxClient.Files.ListFolderAsync(args);
            foreach (var item in result.Entries)
            {
                if (item.IsFile)
                {
                    var f = item.AsFile;
                    SongItem s = new SongItem();
                    s.SourceType = Enums.MusicSource.Dropbox;
                    s.Title = f.Name;
                    s.Path = f.PathLower;
                    s.GenerateID();
                    songs.Add(s);
                }
            }
            return songs;
        }

        public async Task<List<CloudFolder>> GetSubFolders(string id)
        {
            List<CloudFolder> folders = new List<CloudFolder>();
            if (!IsAuthenticated) return folders;
            var args = new Dropbox.Api.Files.ListFolderArg("", false, true, false, false);
            var result = await dropboxClient.Files.ListFolderAsync(args);
            foreach (var item in result.Entries)
            {
                if (item.IsFolder)
                {
                    var f = item.AsFolder;
                    folders.Add(new CloudFolder(f.Name, f.PathLower, 0, f.Id, id, MusicItemTypes.dropboxfolder));
                }
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
