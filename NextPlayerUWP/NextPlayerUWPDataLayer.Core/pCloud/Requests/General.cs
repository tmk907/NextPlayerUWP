using NextPlayerUWPDataLayer.pCloud.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class General : BaseRequest
    {
        public General()
        {
            this.downloader = new Downloader();
        }

        public async Task<DigestResponse> GetDigest()
        {
            var url = $"{BaseUrl}/getdigest";
            var response = await downloader.DownloadJsonAsync<DigestResponse>(url);
            return response;
        }

        public async Task<UserInfoResponse> UserInfo()
        {
            var url = $"{BaseUrl}/userinfo?{authParam}={authToken}";
            var response = await downloader.DownloadJsonAsync<UserInfoResponse>(url);
            return response;
        }

        public async Task SupportedLanguages()
        {
            throw new NotImplementedException();
        }

        public async Task SetLanguage()
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> Feedback(string mail, string reason, string message, string name = "")
        {
            var namePart = (name == "") ? "" : $"&name={name}";
            var url = $"{BaseUrl}/feedback?mail={mail}&reason={reason}&message={message}{namePart}";
            var response = await downloader.DownloadJsonAsync<DigestResponse>(url);
            return response;
        }

        public async Task<CurrentServerResponse> CurrentServer()
        {
            var url = $"{BaseUrl}/currentserver";
            var response = await downloader.DownloadJsonAsync<CurrentServerResponse>(url);
            return response;
        }

        public async Task Diff()
        {
            throw new NotImplementedException();
        }

        public async Task GetFileHistory()
        {
            throw new NotImplementedException();
        }

        public async Task<GetIPResponse> GetIP()
        {
            var url = $"{BaseUrl}/getip";
            var response = await downloader.DownloadJsonAsync<GetIPResponse>(url);
            return response;
        }
    }
}
