using NextPlayerUWPDataLayer.Services;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class FutureAccessHelper
    {
        public static async Task AddToFutureAccessListAndSaveTokenAsync(IStorageItem item)
        {
            if (StorageApplicationPermissions.FutureAccessList.Entries.Count >= StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed - 2)
            {
                string deletedToken = await DatabaseManager.Current.DeleteAccessTokenAsync();
                StorageApplicationPermissions.FutureAccessList.Remove(deletedToken);
            }
            string token = StorageApplicationPermissions.FutureAccessList.Add(item);
            bool isFile = item.IsOfType(StorageItemTypes.File);
            await DatabaseManager.Current.SaveAccessToken(item.Path, token, isFile);
        }

        public static async Task<string> GetTokenFromPathAsync(string path)
        {
            string token = await DatabaseManager.Current.GetAccessToken(path);
            return token;
        }

        public static async Task<StorageFile> GetFileFromPathAsync(string path)
        {
            StorageFile file = null;
            string token = await DatabaseManager.Current.GetAccessToken(path);
            if (!String.IsNullOrEmpty(token))
            {

                try
                {
                    file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                }
                catch (Exception ex)
                {
                }
            }
            return file;
        } 
    }
}
