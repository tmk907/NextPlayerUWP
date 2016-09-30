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
using System.Collections.Concurrent;

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
            Debug.WriteLine("PCloudService() {0}", new object[] { userId });
            this.userId = userId;
            pCloudClient = new PCloudClient();
        }

        private PCloudClient pCloudClient;
        private string accessToken;
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
                    accessToken = response.AccessToken;
                    pCloudClient = new PCloudClient(AuthType.AccessToken, accessToken);
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
                if (pCloudClient == null || accessToken == null) return false;
                else return true;
            }
        }

        public async Task<bool> Login()
        {
            bool isLoggedIn = false;
            if (IsAuthenticated)
            {
                pCloudClient = new PCloudClient(AuthType.AccessToken, accessToken);
                isLoggedIn = true;
            }
            else
            {
                Debug.WriteLine("PCloudService Login()");

                await Authorize();
                isLoggedIn = IsAuthenticated;

                if (IsAuthenticated && userId == null)
                {
                    var account = await pCloudClient.General.UserInfo();
                    userId = account.UserId.ToString();
                    string username = account.Email;
                    if (!CloudAccounts.Instance.Exists(userId, CloudStorageType.pCloud))
                    {
                        await CloudAccounts.Instance.AddAccount(userId, CloudStorageType.pCloud, username);
                    }
                    else
                    {
                        return false;
                    }
                    await SaveToken(accessToken);
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
            if (IsAuthenticated)
            {
                return true;
            }
            accessToken = await GetSavedToken();
            pCloudClient = new PCloudClient(AuthType.AccessToken, accessToken);
            return IsAuthenticated;
        }

        public async Task Logout()
        {
            Debug.WriteLine("PCloudService Logout()");
            accessToken = null;
            //await pCloudClient.Auth.Logout();
            await CloudAccounts.Instance.DeleteAccount(userId, CloudStorageType.pCloud);
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
            return (type == CloudStorageType.pCloud) && userId == this.userId;
        }

        public async Task<CloudAccount> GetAccountInfo()
        {
            if (userId == null) return null;
            return CloudAccounts.Instance.GetAccount(userId, CloudStorageType.pCloud);
        }

        public async Task<string> GetDownloadLink(string fileId)
        {
            await LoginSilently();
            if (!IsAuthenticated) return null;
            var linkResponse = await pCloudClient.Streaming.GetAudioLink(Int32.Parse(fileId));
            var host = linkResponse.Hosts.FirstOrDefault();
            if (host == null) return null;
            var link = "https://" + host + linkResponse.Path;
            return link;
        }


        private ConcurrentDictionary<string, Task<NextPlayerUWPDataLayer.pCloud.Model.BaseMetadata>> cache = new ConcurrentDictionary<string, Task<NextPlayerUWPDataLayer.pCloud.Model.BaseMetadata>>();
        private ConcurrentDictionary<string, Task<CloudFolder>> cachedFolders = new ConcurrentDictionary<string, Task<CloudFolder>>();

        public void ClearCache()
        {
            Debug.WriteLine("OneDriveService ClearCache()");
            cache = new ConcurrentDictionary<string, Task<NextPlayerUWPDataLayer.pCloud.Model.BaseMetadata>>();
            cachedFolders = new ConcurrentDictionary<string, Task<CloudFolder>>();
        }

        public async Task<string> GetRootFolderId()
        {
            Debug.WriteLine("PCloudService GetRootFolderId");
            if (!cachedFolders.ContainsKey("0"))
            {
                var folder = await GetFolder("0");
            }
            return "0";
        }

        public Task<CloudFolder> GetFolder(string folderId)
        {
            Debug.WriteLine("PCloudService GetFolder {0}", new object[] { folderId });
            return cachedFolders.GetOrAdd(folderId, GetFolderInternalAsync);
        }

        public async Task<List<SongItem>> GetSongItems(string folderId)
        {
            Debug.WriteLine("PCloudService GetSongItems {0}", new object[] { folderId });
            var contentTask = cache.GetOrAdd(folderId, GetFolderContentInternalAsync);
            var folderTask = cachedFolders.GetOrAdd(folderId, GetFolderInternalAsync);

            var content = await contentTask;
            var folder = await folderTask;

            List<SongItem> songs = new List<SongItem>();
            List<SongData> songData = new List<SongData>();
            if (content == null)
            {
                ClearCache();
                return songs;
            }
            foreach (var item in content.Contents)
            {
                if (!item.IsFolder)
                {
                    if (item.Name.ToLower().EndsWith(".mp3") || item.Name.ToLower().EndsWith(".m4a") ||
                        item.Name.ToLower().EndsWith(".wma") || item.Name.ToLower().EndsWith(".flac"))
                    {
                        songData.Add(CreateSongData(item, userId, folder));
                    }
                }
            }

            songs = await DatabaseManager.Current.InsertCloudItems(songData, Enums.MusicSource.PCloud);

            return songs;
        }

        public async Task<List<CloudFolder>> GetSubFolders(string folderId)
        {
            Debug.WriteLine("PCloudService GetSubFolders {0}", new object[] { folderId });

            var contentTask = cache.GetOrAdd(folderId, GetFolderContentInternalAsync);
            var content = await contentTask;

            List<CloudFolder> folders = new List<CloudFolder>();
            if (content == null)
            {
                ClearCache();
                return folders;
            }
            foreach (var item in content.Contents)
            {
                if (item.IsFolder)
                {
                    folders.Add(new CloudFolder(item.Name, item.Path, 0, item.FolderId.ToString(), item.ParentFolderId.ToString(), CloudStorageType.pCloud, userId));
                }
            }
            return folders;
        }

        private async Task<CloudFolder> GetFolderInternalAsync(string folderId)
        {
            Debug.WriteLine("PCloudService GetFolderInternalAsync() {0}", new object[] { folderId });
            await LoginSilently();
            if (!IsAuthenticated) return null;
            try
            {
                var response = await pCloudClient.Folder.ListFolder(Int32.Parse(folderId), false, false, true, true);
                var item = response.Metadata;
                if (item == null) return null;
                CloudFolder folder = new CloudFolder(item.Name, item.Path, 0, item.FolderId.ToString(), item.ParentFolderId.ToString(), CloudStorageType.pCloud, userId);
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

        private async Task<NextPlayerUWPDataLayer.pCloud.Model.BaseMetadata> GetFolderContentInternalAsync(string folderId)
        {
            Debug.WriteLine("PCloudService GetFolderContentInternalAsync() {0}", new object[] { folderId });
            await LoginSilently();
            if (!IsAuthenticated) return null;
            try
            {
                var response = await pCloudClient.Folder.ListFolder(Int32.Parse(folderId));
                var result = response.Metadata;
                return result;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private static SongData CreateSongData(NextPlayerUWPDataLayer.pCloud.Model.BaseMetadata item, string userId, CloudFolder folder)
        {
            SongData song = new SongData();
            //song.AlbumArtPath = 
            song.Bitrate = 0;
            song.CloudUserId = userId;
            song.DateAdded = DateTime.Now;
            song.DateModified = item.Modified;
            song.Duration = TimeSpan.Zero;
            song.Filename = item.Name;
            song.FileSize = (ulong)item.Size;
            song.IsAvailable = 0;
            song.LastPlayed = DateTime.MinValue;
            song.MusicSourceType = (int)Enums.MusicSource.PCloud;
            song.Path = item.FileId.ToString();
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
