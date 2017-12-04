using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands.Navigation
{
    public class AddToPlaylistCommand : IGenericCommand
    {
        private MusicItem item;

        public AddToPlaylistCommand(MusicItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            App.Current.NavigationService.Navigate(AppPages.Pages.AddToPlaylist, item.GetParameter());
            await Task.CompletedTask;
        }
    }
}
