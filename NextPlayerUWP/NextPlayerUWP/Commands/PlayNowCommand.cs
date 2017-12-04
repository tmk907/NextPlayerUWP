using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class PlayNowCommand : IGenericCommand
    {
        private MusicItem item;

        public PlayNowCommand(MusicItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            await PlaybackService.Instance.PlayNewList(0);
        }
    }
}
