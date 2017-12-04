using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class PlayNowCommand : IGenericCommand
    {
        private MusicItem item;
        private IEnumerable<MusicItem> items;

        public PlayNowCommand(MusicItem item)
        {
            this.item = item;
        }

        public PlayNowCommand(IEnumerable<MusicItem> items)
        {
            this.items = items;
        }

        public async Task Excecute()
        {
            if (item != null)
            {
                await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            }
            else
            {
                await NowPlayingPlaylistManager.Current.NewPlaylist(items)
            }
            await PlaybackService.Instance.PlayNewList(0);
        }
    }
}
