using NextPlayerUWPDataLayer.Services;
using System.Threading.Tasks;

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
            await DatabaseManager.Current.SaveAccessToken(path, token, isFile);
        }
    }
}
