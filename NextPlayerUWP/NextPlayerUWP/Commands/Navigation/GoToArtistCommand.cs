using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands.Navigation
{
    public class GoToArtistCommand : IGenericCommand
    {
        private SongItem item;

        public GoToArtistCommand(SongItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            ArtistItem temp = await DatabaseManager.Current.GetArtistItemAsync(item.FirstArtist);
            App.Current.NavigationService.Navigate(AppPages.Pages.Artist, temp.ArtistId);
        }
    }
}
