using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists.ContentCreator
{
    public interface IContentCreator
    {
        string CreateContent(PlaylistItem item, IEnumerable<SongItem> songs);

        Task UpdateContent(PlaylistItem item, IEnumerable<SongItem> songs, StorageFile file);
    }
}
