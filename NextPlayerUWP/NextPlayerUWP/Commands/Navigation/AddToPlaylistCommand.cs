using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands.Navigation
{
    public class AddToPlaylistCommand : IGenericCommand
    {
        private MusicItem item;
        private IEnumerable<MusicItem> items;

        public AddToPlaylistCommand(MusicItem item)
        {
            this.item = item;
        }

        public AddToPlaylistCommand(IEnumerable<MusicItem> items)
        {
            this.items = items;
        }

        public async Task Excecute()
        {
            if (item != null)
            {
                App.Current.NavigationService.Navigate(AppPages.Pages.AddToPlaylist, item.GetParameter());
            }
            else
            {
                App.AddToCache(items);
                App.Current.NavigationService.Navigate(AppPages.Pages.AddToPlaylist, new ListOfMusicItems().GetParameter());
            }
            await Task.CompletedTask;
        }
    }
}
