namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class BaseRequest
    {
        protected Downloader downloader;
        protected string authToken;
        protected readonly string BaseUrl = "https://api.pcloud.com";
        protected string authParam = "auth=";

        public BaseRequest()
        {
            this.downloader = new Downloader();
        }

        public void SetDownloader(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public void SetAuth(AuthType type, string token)
        {
            authToken = token;

            if (type == AuthType.AccessToken)
            {
                authParam = "access_token";
            }
            else if (type == AuthType.AuthToken)
            {
                authParam = "auth";
            }
        }
    }
}
