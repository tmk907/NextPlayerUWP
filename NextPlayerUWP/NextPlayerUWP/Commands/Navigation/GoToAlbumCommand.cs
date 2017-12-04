using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands.Navigation
{
    public class GoToAlbumCommand : IGenericCommand
    {
        private SongItem item;

        public GoToAlbumCommand(SongItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            AlbumItem temp = await DatabaseManager.Current.GetAlbumItemAsync(item.Album, item.AlbumArtist);
            App.Current.NavigationService.Navigate(AppPages.Pages.Album, temp.AlbumId);
        }
    }
}
