using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists.ContentCreator
{
    public class EmptyContentCreator : IContentCreator
    {
        public string CreateContent(PlaylistItem item, IEnumerable<SongItem> songs)
        {
            return "";
        }

        public Task UpdateContent(PlaylistItem item, IEnumerable<SongItem> songs, StorageFile file)
        {
            return Task.CompletedTask;
        }
    }
}
