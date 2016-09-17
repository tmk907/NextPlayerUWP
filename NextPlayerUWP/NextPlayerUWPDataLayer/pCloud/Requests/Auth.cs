using NextPlayerUWPDataLayer.pCloud.Model;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class Auth :BaseRequest
    {
        public Auth()
        {
            this.downloader = new Downloader();
        }

        public async Task<LogoutResponse> Logout()
        {
            var url = $"{BaseUrl}/logout?{authParam}={authToken}";
            var response = await downloader.DownloadJsonAsync<LogoutResponse>(url);
            return response;
        }

        public async Task<ListTokensResponse> ListTokens()
        {
            var url = $"{BaseUrl}/listtokens?{authParam}={authToken}";
            var response = await downloader.DownloadJsonAsync<ListTokensResponse>(url);
            return response;
        }

        public async Task<BaseResponse> DeleteToken(int tokenId)
        {
            var url = $"{BaseUrl}/deletetoken?{authParam}={authToken}&tokenid={tokenId}";
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
