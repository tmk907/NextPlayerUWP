using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace NextPlayerUWP.Common
{
    public class MessageDialogHelper
    {
        TranslationHelper translator;
        public MessageDialogHelper()
        {
            translator = new TranslationHelper();
        }

        public async Task AlbumArtSaveError()
        {
            string content = translator.GetTranslation(TranslationHelper.AlbumArtSaveError);
            await ShowDialog(content);
        }

        public async Task ShowDialog(string content)
        {
            MessageDialog dialog = new MessageDialog(content);
            await dialog.ShowAsync();
        }

        public async Task ShowDialog(string content, string title)
        {
            MessageDialog dialog = new MessageDialog(content, title);
            await dialog.ShowAsync();
        }

        public async Task<bool> IncludeAllSubFolders()
        {
            string content = translator.GetTranslation(TranslationHelper.IncludeSubFolders);
            MessageDialog dialog = new MessageDialog(content);
            dialog.Commands.Add(new UICommand(translator.GetTranslation(TranslationHelper.Yes)) { Id = 0 });
            dialog.Commands.Add(new UICommand(translator.GetTranslation(TranslationHelper.No)) { Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var command = await dialog.ShowAsync();
            return (int)command.Id == 0;
        }
    }
}
