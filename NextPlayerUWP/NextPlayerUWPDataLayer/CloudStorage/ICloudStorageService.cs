using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.CloudStorage
{
    public delegate void AuthenticationChangeHandler(bool isAuthenticated);

    public interface ICloudStorageService
    {
        Task<bool> Login();
        Task<bool> LoginSilently();
        Task Logout();
        Task<CloudAccount> GetAccountInfo();

        //CloudRootFolder GetRootFolder();
        Task<string> GetRootFolderId();
        Task<CloudFolder> GetFolder(string id);
        Task<List<CloudFolder>> GetSubFolders(string id);
        Task<List<SongItem>> GetSongItems(string id);
        Task<string> GetDownloadLink(string id);

        bool Check(string userId, CloudStorageType type);
    }
}
