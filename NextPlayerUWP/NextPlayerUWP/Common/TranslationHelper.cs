using NextPlayerUWPDataLayer.Helpers;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Common
{
    public class TranslationHelper
    {
        public const string OK = "OK";
        public const string Yes = "Yes";
        public const string No = "No"; 
        public const string Delete = "Delete";
        public const string Cancel = "Cancel";
        public const string AlbumArtSaveError = "AlbumArtSaveError";
        public const string DeletePlaylistConfirmation = "DeletePlaylistConfirmation";
        public const string UnknownAlbum = "UnknownAlbum";
        public const string UnknownAlbumArtist = "UnknownAlbumArtist";
        public const string UnknownArtist = "UnknownArtist";
        public const string ConnectionError = "ConnectionError";
        public const string CantFindLyrics = "CantFindLyrics";
        public const string DoYouWantToIncludeSubFolders = "DoYouWantToIncludeSubFolders";
        public const string IncludeSubfolders = "IncludeSubfolders";
        public const string DontIncludeSubFolders = "DontIncludeSubFolders";
        public const string DoYouWantExcludeFolderFromLibrary = "DoYouWantExcludeFolderFromLibrary";
        public const string DoYouWantDeleteThisSong = "DoYouWantDeleteThisSong";
        public const string AddedNext = "Added next";
        public const string AddedToNowPlaying = "Added to now playing";

        private ResourceLoader loader;

        public TranslationHelper()
        {
            loader = new ResourceLoader();
        }

        public string GetTranslation(string toTranslate)
        {
            return loader.GetString(toTranslate);
        }

        public void ChangeSlideableItemDescription()
        {
            string swipeAction = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand) as string;
            string translation = "";
            Symbol symbol = Symbol.Play;

            switch (swipeAction)
            {
                case SettingsKeys.SwipeActionPlayNow:
                    translation = GetTranslation("Play now");
                    symbol = Symbol.Play;
                    break;
                case SettingsKeys.SwipeActionPlayNext:
                    translation = GetTranslation("Play next");
                    symbol = Symbol.Add;
                    break;
                case SettingsKeys.SwipeActionAddToNowPlaying:
                    translation = GetTranslation("Add to now playing");
                    symbol = Symbol.Add;
                    break;
                case SettingsKeys.SwipeActionAddToPlaylist:
                    translation = GetTranslation("Add to playlist");
                    symbol = Symbol.Add;
                    break;
                default:
                    break;
            }
            foreach (var dict in App.Current.Resources.ThemeDictionaries)
            {
                var theme = dict.Value as Windows.UI.Xaml.ResourceDictionary;
                theme["SlideableListItemLeftLabel"] = translation;
                theme["SlideableListItemLeftIcon"] = symbol;
            }
        }
    }
}
