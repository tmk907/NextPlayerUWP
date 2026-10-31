using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NextPlayerWinUI.Common
{
    public class MessageDialogHelper
    {
        // A window can show only one ContentDialog at a time.
        private static readonly SemaphoreSlim dialogLock = new SemaphoreSlim(1, 1);

        private readonly TranslationHelper translator = new TranslationHelper();

        public async Task ShowAlbumArtSaveError()
        {
            await ShowDialog(translator.GetTranslation(TranslationHelper.AlbumArtSaveError));
        }

        public Task ShowDialog(string content) => ShowDialog(content, null);

        public async Task ShowDialog(string content, string title)
        {
            await ShowAsync(new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = translator.GetTranslation(TranslationHelper.OK),
            });
        }

        public Task<bool> ShowDeleteSongConfirmationDialog()
            => AskAsync(translator.GetTranslation(TranslationHelper.DoYouWantDeleteThisSong));

        public Task<bool> ShowIncludeAllSubFoldersQuestion()
            => AskAsync(translator.GetTranslation(TranslationHelper.DoYouWantToIncludeSubFolders));

        public Task<bool> ShowExcludeFolderConfirmation()
            => AskAsync(translator.GetTranslation(TranslationHelper.DoYouWantExcludeFolderFromLibrary));

        private async Task<bool> AskAsync(string content)
        {
            var result = await ShowAsync(new ContentDialog
            {
                Content = content,
                PrimaryButtonText = translator.GetTranslation(TranslationHelper.Yes),
                CloseButtonText = translator.GetTranslation(TranslationHelper.No),
                DefaultButton = ContentDialogButton.Primary,
            });
            return result == ContentDialogResult.Primary;
        }

        private static async Task<ContentDialogResult> ShowAsync(ContentDialog dialog)
        {
            await dialogLock.WaitAsync();
            try
            {
                dialog.XamlRoot = WindowHelper.XamlRoot;
                return await dialog.ShowAsync();
            }
            finally
            {
                dialogLock.Release();
            }
        }
    }
}
