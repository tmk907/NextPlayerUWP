using System.Collections.Generic;
using NextPlayerUWPDataLayer.Model;
using PlaylistsNET.Model;
using PlaylistsNET.Content;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace NextPlayerUWPDataLayer.Playlists.ContentCreator
{
    public class PlsContentCreator : IContentCreator
    {
        public string CreateContent(PlaylistItem item, IEnumerable<SongItem> songs)
        {
            var playlist = new PlsPlaylist();
            foreach (var song in songs)
            {
                playlist.PlaylistEntries.Add(new PlsPlaylistEntry()
                {
                    Length = song.Duration,
                    Path = song.Path,
                    Title = song.Title
                });
            }
            var creator = new PlsContent();
            string content = creator.Create(playlist);
            return content;
        }

        public async Task UpdateContent(PlaylistItem item, IEnumerable<SongItem> songs, StorageFile file)
        {
            PlaylistFileReader pr = new PlaylistFileReader();
            var playlist = await pr.OpenPlsPlaylist(file);
            if (playlist.PlaylistEntries.Count > 0)
            {
                if (playlist.PlaylistEntries.FirstOrDefault(e => e.Path.Contains(System.IO.Path.VolumeSeparatorChar)) == null)
                {
                    string folderPath = System.IO.Path.GetDirectoryName(file.Path);
                    foreach (var song in songs)
                    {
                        song.Path = PlaylistsNET.Utils.Utils.MakeRelativePath(folderPath, song.Path);
                    }
                }
            }
            string content = CreateContent(item, songs);
            try
            {
                await FileIO.WriteTextAsync(file, content);
            }
            catch (Exception ex)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage(ex.ToString());
            }
        }
    }
}
