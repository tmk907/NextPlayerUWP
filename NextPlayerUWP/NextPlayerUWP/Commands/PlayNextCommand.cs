using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class PlayNextCommand : IGenericCommand
    {
        private MusicItem item;
        private IEnumerable<MusicItem> items;

        public PlayNextCommand(MusicItem item)
        {
            this.item = item;
        }

        public PlayNextCommand(IEnumerable<MusicItem> items)
        {
            this.items = items;
        }

        public async Task Excecute()
        {
            if (item != null)
            {
                await NowPlayingPlaylistManager.Current.AddNext(item);
            }
            else
            {
                await NowPlayingPlaylistManager.Current.AddNext(items);
            }
        }
    }
}
