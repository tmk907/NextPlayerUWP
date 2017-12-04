using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class AddToNowPlayingCommand : IGenericCommand
    {
        private MusicItem item;
        private IEnumerable<MusicItem> items;

        public AddToNowPlayingCommand(MusicItem item)
        {
            this.item = item;
        }

        public AddToNowPlayingCommand(IEnumerable<MusicItem> items)
        {
            this.items = items;
        }

        public async Task Excecute()
        {
            if (item != null)
            {
                await NowPlayingPlaylistManager.Current.Add(item);
            }
            else
            {
                await NowPlayingPlaylistManager.Current.Add(items);
            }
        }
    }
}
