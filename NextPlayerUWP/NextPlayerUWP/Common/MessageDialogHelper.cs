using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace NextPlayerUWP.Common
{
    public class MessageDialogHelper
    {
        TranslationHelper translationHelper;
        public MessageDialogHelper()
        {
            translationHelper = new TranslationHelper();
        }

        public async Task AlbumArtSaveError()
        {
            string content = translationHelper.GetTranslation(TranslationHelper.AlbumArtSaveError);
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
    }
}
