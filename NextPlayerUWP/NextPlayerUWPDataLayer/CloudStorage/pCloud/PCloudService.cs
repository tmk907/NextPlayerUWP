using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using System.Diagnostics;
using NextPlayerUWPDataLayer.pCloud;
using Windows.Security.Authentication.Web;
using NextPlayerUWPDataLayer.Services;
using NextPlayerUWPDataLayer.Constants;

namespace NextPlayerUWPDataLayer.CloudStorage.pCloud
{
    public class PCloudService : ICloudStorageService
    {
        public PCloudService()
        {
            Debug.WriteLine("PCloudService()");
            pCloudClient = new PCloudClient();
        }

        public PCloudService(string userId)
        {
            Debug.WriteLine("PCloudService() {0}", userId);
            this.userId = userId;
            pCloudClient = new PCloudClient();
        }

        private PCloudClient pCloudClient;
        private string refreshToken;
        private string userId;
        private static readonly Uri redirectUri = new Uri("http://localhost/next-player/pcloud/authorize");

        private async Task Authorize()
        {
            var authUri = pCloudClient.GetAuthorizeUri(AppConstants.PCloudClientId, AuthFlow.Token, redirectUri);
            try
            {
                var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, authUri, redirectUri);
                ProcessResult(result);
            }
            catch (Exception ex)
            {

            }
        }

        private void ProcessResult(WebAuthenticationResult result)
        {
            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    var response = pCloudClient.ParseTokenFlowResponse(result.ResponseData);
                    refreshToken = response.AccessToken;
                    pCloudClient = new PCloudClient(refreshToken);
                    break;
                case WebAuthenticationStatus.ErrorHttp:
                    //throw new OAuthException(result.ResponseErrorDetail);

                case WebAuthenticationStatus.UserCancel:
                default:
                    //throw new OAuthUserCancelledException();
                    break;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                if (pCloudClient == null || refreshToken == null) return false;
                else return true;
            }
        }

        public async Task<bool> Login()
        {
            Debug.WriteLine("PCloudService Login()");

            bool isLoggedIn = false;
            if (IsAuthenticated)
            {
                pCloudClient = new PCloudClient(refreshToken);
                isLoggedIn = true;
            }
            else
            {
                await Authorize();
                isLoggedIn = IsAuthenticated;

                if (IsAuthenticated && userId == null)
                {
                    var account = await pCloudClient.General.UserInfo();
                    userId = account.UserId.ToString();
                    string username = account.Email;
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

        public Task<bool> Login(string login, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> LoginSilently()
        {
            Debug.WriteLine("PCloudService LoginSilently()");
            refreshToken = await GetSavedToken();
            pCloudClient = new PCloudClient(refreshToken);
            return IsAuthenticated;
        }

        public async Task Logout()
        {
            Debug.WriteLine("PCloudService Logout()");
            refreshToken = null;
            await pCloudClient.Auth.Logout();
            await CloudAccounts.Instance.DeleteAccount(userId, CloudStorageType.Dropbox);
        }

        private async Task SaveToken(string refreshToken)
        {
            Debug.WriteLine("PCloudService SaveToken()");
            await DatabaseManager.Current.SaveCloudAccountTokenAsync(userId, refreshToken);
        }

        private async Task<string> GetSavedToken()
        {
            Debug.WriteLine("PCloudService GetSavedToken()");
            return await DatabaseManager.Current.GetCloudAccountTokenAsync(userId);
        }


        public bool Check(string userId, CloudStorageType type)
        {
            return (type == CloudStorageType.Dropbox) && userId == this.userId;
        }

        public async Task<CloudAccount> GetAccountInfo()
        {
            if (userId == null) return null;
            return CloudAccounts.Instance.GetAccount(userId);
        }

        public Task<string> GetDownloadLink(string id)
        {
            throw new NotImplementedException();
        }

        public Task<CloudFolder> GetFolder(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetRootFolderId()
        {
            return "0";
        }

        public Task<List<SongItem>> GetSongItems(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<CloudFolder>> GetSubFolders(string id)
        {
            throw new NotImplementedException();
        }

        
    }
}
