using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands.Navigation
{
    public class ShowDetailsCommand : IGenericCommand
    {
        private MusicItem item;
        
        public ShowDetailsCommand(MusicItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            App.Current.NavigationService.Navigate(AppPages.Pages.FileInfo, item.GetParameter());
            await Task.CompletedTask;
        }
    }
}
