using NextPlayerUWPDataLayer.pCloud.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud
{
    public class AuthFlow
    {
        public static readonly string Code = "code";
        public static readonly string Token = "token";
    }

    public class TokenFlowResponse
    {
        public TokenFlowResponse(string accessToken, string tokenType, int userId, string state)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            UserId = userId;
            State = state;
        }

        public string AccessToken { get;}
        public string TokenType { get; }
        public int UserId { get; }
        public string State { get; }
    }

    public class PCloudClient
    {
        private string refreshToken;
        private string authToken;

        public PCloudClient()
        {

        }

        public PCloudClient(string refreshToken)
        {
            this.refreshToken = refreshToken;
        }

        public void SetRefreshToken(string token)
        {
            authToken = refreshToken;
        }

        public Uri GetAuthorizeUri(string clientId, string authFlow, Uri redirectUri = null, string state = null, bool forceReapprove = false)
        {
            if (redirectUri == null && authFlow == AuthFlow.Token)
            {
                return null;//Error
            }
            string oauthUrl = "https://my.pcloud.com/oauth2";
            StringBuilder sb = new StringBuilder();
            if (authFlow == AuthFlow.Token)
            {
                sb.Append("&redirect_uri=");
                sb.Append(redirectUri.ToString());
            }
            else if(redirectUri != null)
            {
                sb.Append("&redirect_uri=");
                sb.Append(redirectUri);
            }
            if (state != null)
            {
                sb.Append("&state=");
                sb.Append(state);
            }
            if (forceReapprove)
            {
                sb.Append("&force_reapprove=");
                sb.Append(forceReapprove);
            }
            string optionalParams = sb.ToString();
            var url = $"{oauthUrl}/authorize?client_id={clientId}&response_type={authFlow}{optionalParams}";
            return new Uri(url);
        }

        public TokenFlowResponse ParseTokenFlowResponse(string redirectUri)
        {
            string querystring = redirectUri.Substring(redirectUri.IndexOf('?') + 1);
            var queryParams = querystring.Split(new char[]{ '#' })[1].Split(new char[] { '&' },StringSplitOptions.RemoveEmptyEntries);
            string accessToken = "";
            string tokenType = "";
            int userId = -1;
            string state = "";
            foreach (var p in queryParams)
            {
                var a = p.Split(new char[] { '=' });
                switch (a[0])
                {
                    case "access_token":
                        accessToken = a[1];
                        break;
                    case "token_type":
                        tokenType = a[1];
                        break;
                    case "userid":
                        userId = Int32.Parse(a[1]);
                        break;
                    case "state":
                        state = a[1];
                        break;
                    default:
                        break;
                }
            }
            TokenFlowResponse tfr = new TokenFlowResponse(accessToken, tokenType, userId, state);
            return tfr;
        }

        private General general;
        public General General
        {
            get
            {
                if (general == null)
                {
                    general = new General(authToken);
                }
                return general;
            }
        }

        private Auth auth;
        public Auth Auth
        {
            get
            {
                if (auth == null)
                {
                    auth = new Auth(authToken);
                }
                return auth;
            }
        }

        private Folder folder;
        public Folder Folder
        {
            get
            {
                if (folder == null)
                {
                    folder = new Folder(authToken);
                }
                return folder;
            }
        }

        private Streaming streaming;
        public Streaming Streaming
        {
            get
            {
                if (streaming == null)
                {
                    streaming = new Streaming(authToken);
                }
                return streaming;
            }
        }

        private Collection collection;
        public Collection Collection
        {
            get
            {
                if (collection == null)
                {
                    collection = new Collection(authToken);
                }
                return collection;
            }
        }
    }
}
