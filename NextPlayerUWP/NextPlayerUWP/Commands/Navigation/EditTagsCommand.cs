using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;

namespace NextPlayerUWP.Commands.Navigation
{
    public class EditTagsCommand : IGenericCommand
    {
        private MusicItem item;

        public EditTagsCommand(MusicItem item)
        {
            this.item = item;
        }

        public async Task Excecute()
        {
            App.Current.NavigationService.Navigate(AppPages.Pages.TagsEditor, item.GetParameter());
            await Task.CompletedTask;
        }
    }
}
