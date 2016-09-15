using NextPlayerUWPDataLayer.pCloud.Model;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public  class Auth
    {
        private Downloader downloader;
        private string authToken;
        private readonly string BaseUrl = "https://api.pcloud.com";

        public Auth(string authToken)
        {
            this.authToken = authToken;
            this.downloader = new Downloader();
        }

        public void SetDownloader(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public async Task<LogoutResponse> Logout()
        {
            var url = $"{BaseUrl}/logout?auth={authToken}";
            var response = await downloader.DownloadJsonAsync<LogoutResponse>(url);
            return response;
        }

        public async Task<ListTokensResponse> ListTokens()
        {
            var url = $"{BaseUrl}/listtokens?auth={authToken}";
            var response = await downloader.DownloadJsonAsync<ListTokensResponse>(url);
            return response;
        }

        public async Task<BaseResponse> DeleteToken(int tokenId)
        {
            var url = $"{BaseUrl}/deletetoken?auth={authToken}&tokenid={tokenId}";
            var response = await downloader.DownloadJsonAsync<BaseResponse>(url);
            return response;
        }

        //sendverificationemail
        //verifyemail
        //changepassword
        //lostpassword
        //resetpassword
        //register
        //invite
        //userinvites
    }
}
