using NextPlayerUWPDataLayer.Model;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists
{
    public interface IPlaylistParser
    {
        Task<ImportedPlaylist> ParsePlaylist(StorageFile file);
    }
}
