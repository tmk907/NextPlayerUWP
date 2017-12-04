using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class PlayNextCommand : IGenericCommand
    {
        private MusicItem item;

        public PlayNextCommand(MusicItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            await NowPlayingPlaylistManager.Current.AddNext(item);
        }
    }
}
