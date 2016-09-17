using NextPlayerUWPDataLayer.pCloud.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud
{
    public enum AuthFlow
    {
        Code,
        Token,
    }

    public enum AuthType
    {
        AccessToken,
        AuthToken,
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
        public string AccessToken { get; private set; }
        public string AuthenticationToken { get; private set; }
        public AuthType AuthType { get; private set; }

        public PCloudClient()
        {

        }

        public PCloudClient(AuthType type, string accessToken)
        {
            AuthType = type;
            this.AccessToken = accessToken;
        }

        public void SetAccessToken(string accessToken)
        {
            AuthType = AuthType.AccessToken;
            this.AccessToken = accessToken;
            if (general != null) General.SetAuth(AuthType.AccessToken, accessToken);
            if (auth != null) Auth.SetAuth(AuthType.AccessToken, accessToken);
            if (folder != null) Folder.SetAuth(AuthType.AccessToken, accessToken);
            if (streaming != null) Streaming.SetAuth(AuthType.AccessToken, accessToken);
            if (collection != null) Collection.SetAuth(AuthType.AccessToken, accessToken);
        }

        public void SetAuthenticationToken(string authenticationToken)
        {
            AuthType = AuthType.AuthToken;
            this.AuthenticationToken = authenticationToken;
            if (general != null) General.SetAuth(AuthType.AuthToken, authenticationToken);
            if (auth != null) Auth.SetAuth(AuthType.AuthToken, authenticationToken);
            if (folder != null) Folder.SetAuth(AuthType.AuthToken, authenticationToken);
            if (streaming != null) Streaming.SetAuth(AuthType.AuthToken, authenticationToken);
            if (collection != null) Collection.SetAuth(AuthType.AuthToken, authenticationToken);
        }

        public Uri GetAuthorizeUri(string clientId, AuthFlow authFlow, Uri redirectUri = null, string state = null, bool forceReapprove = false)
        {
            if (redirectUri == null && authFlow == AuthFlow.Token)
            {
                return null;//Error
            }
            string oauthUrl = "https://my.pcloud.com/oauth2";
            StringBuilder sb = new StringBuilder();
            string authFlowValue = (authFlow == AuthFlow.Code) ? "code" : (authFlow == AuthFlow.Token) ? "token" : "";
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
            var url = $"{oauthUrl}/authorize?client_id={clientId}&response_type={authFlowValue}{optionalParams}";
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
                    general = new General();
                    general.SetAuth(AuthType.AccessToken, AccessToken);
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
                    auth = new Auth();
                    auth.SetAuth(AuthType.AccessToken, AccessToken);
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
                    folder = new Folder();
                    folder.SetAuth(AuthType.AccessToken, AccessToken);
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
                    streaming = new Streaming();
                    streaming.SetAuth(AuthType.AccessToken, AccessToken);
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
                    collection = new Collection();
                    collection.SetAuth(AuthType.AccessToken, AccessToken);
                }
                return collection;
            }
        }
    }
}
