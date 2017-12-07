using NextPlayerUWP.Common;
using NextPlayerUWP.Playback;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class DeleteFromNowPlayingCommand : IGenericCommand
    {
        private SongItem item;

        public DeleteFromNowPlayingCommand(SongItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            await NowPlayingPlaylistManager.Current.Delete(item.SongId);
        }
    }
}
