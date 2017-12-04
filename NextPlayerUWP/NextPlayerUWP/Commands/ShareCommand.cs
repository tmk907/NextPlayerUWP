using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class ShareCommand : IGenericCommand
    {
        private MusicItem item;
        private IEnumerable<MusicItem> items;
        private ShareHelper shareHelper;

        public ShareCommand(MusicItem item)
        {
            this.item = item;
            shareHelper = new ShareHelper();
        }

        public ShareCommand(IEnumerable<MusicItem> items)
        {
            this.items = items;
            shareHelper = new ShareHelper();
        }

        public async Task Excecute()
        {
            if (item != null)
            {
                await shareHelper.Share(item);
            }
            else
            {
                await shareHelper.Share(items);
            }
        }
    }
}
