using NextPlayerUWPDataLayer.Services;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class FutureAccessHelper
    {
        public static async Task<string> GetTokenFromPath(string path)
        {
            string token = await DatabaseManager.Current.GetAccessToken(path);
            return token;
        }

        public static async Task SaveToken(string path, string token, bool isFile = true)
        {
            if (StorageApplicationPermissions.FutureAccessList.Entries.Count >= StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed - 1)
            {
                string deletedToken = await DatabaseManager.Current.DeleteAccessTokenAsync();
                StorageApplicationPermissions.FutureAccessList.Remove(deletedToken);
            }
            await DatabaseManager.Current.SaveAccessToken(path, token, isFile);
        }
    }
}
