using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class PinCommand : IGenericCommand
    {
        private MusicItem item;
        private TilesManager tilesManager;

        public PinCommand(MusicItem item)
        {
            this.item = item;
            tilesManager = new TilesManager();
        }

        public async Task Excecute()
        {
            await tilesManager.CreateTile(item);
        }
    }
}
