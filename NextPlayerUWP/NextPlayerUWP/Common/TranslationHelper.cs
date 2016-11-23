using Windows.ApplicationModel.Resources;

namespace NextPlayerUWP.Common
{
    public class TranslationHelper
    {
        public const string Delete = "Delete";
        public const string Cancel = "Cancel";
        public const string AlbumArtSaveError = "AlbumArtSaveError";
        public const string DeletePlaylistConfirmation = "DeletePlaylistConfirmation";
        public const string UnknownAlbum = "UnknownAlbum";
        public const string UnknownArtist = "UnknownArtist";
        public const string ConnectionError = "ConnectionError";
        public const string CantFindLyrics = "CantFindLyrics";

        private ResourceLoader loader;

        public TranslationHelper()
        {
            loader = new ResourceLoader();
        }

        public string GetTranslation(string toTranslate)
        {
            return loader.GetString(toTranslate);
        }
    }
}
