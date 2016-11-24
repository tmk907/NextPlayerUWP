using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using Windows.Storage;
using System;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class UnsupportedPlaylistParser : BasePlaylistParser
    {
        public override async Task<ImportedPlaylist> ParsePlaylist(StorageFile file)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.Name = file.Name;
            iplaylist.Path = file.Path;
            var prop = await file.GetBasicPropertiesAsync();
            DateTime dateModified = prop.DateModified.UtcDateTime;
            iplaylist.DateModified = dateModified;
            return iplaylist;
        }
    }
}
