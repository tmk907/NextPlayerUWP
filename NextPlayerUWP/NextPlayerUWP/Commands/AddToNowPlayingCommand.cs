using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class AddToNowPlayingCommand : IGenericCommand
    {
        private MusicItem item;

        public AddToNowPlayingCommand(MusicItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            await NowPlayingPlaylistManager.Current.Add(item);
        }
    }
}
