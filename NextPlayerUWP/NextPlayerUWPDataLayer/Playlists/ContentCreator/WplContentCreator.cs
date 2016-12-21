using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using Windows.Storage;
using PlaylistsNET.Model;
using PlaylistsNET.Content;
using System.IO;
using System.Xml.Linq;

namespace NextPlayerUWPDataLayer.Playlists.ContentCreator
{
    public class WplContentCreator : IContentCreator
    {
        public string CreateContent(PlaylistItem item, IEnumerable<SongItem> songs)
        {
            var playlist = new WplPlaylist();
            playlist.Title = item.Name;
            foreach (var song in songs)
            {
                playlist.PlaylistEntries.Add(new WplPlaylistEntry()
                {
                    AlbumTitle = song.Album,
                    AlbumArtist = song.AlbumArtist,
                    Duration = song.Duration,
                    Path = song.Path,
                    TrackArtist = song.Artist,
                    TrackTitle = song.Title
                });
            }
            var creator = new WplContent();
            string content = creator.Create(playlist);
            return content;
        }

        public async Task UpdateContent(PlaylistItem item, IEnumerable<SongItem> songs, StorageFile file)
        {
            var wplContent = new WplContent();
            PlaylistFileReader pr = new PlaylistFileReader();
            var playlist = await pr.OpenWplPlaylist(file);
            playlist.Title = item.Name;
            if (playlist.PlaylistEntries.Count > 0)
            {
                if (playlist.PlaylistEntries.FirstOrDefault(e => e.Path.Contains(Path.VolumeSeparatorChar)) == null)
                {
                    string folderPath = Path.GetDirectoryName(file.Path);
                    foreach (var song in songs)
                    {
                        song.Path = PlaylistsNET.Utils.Utils.MakeRelativePath(folderPath, song.Path);
                    }
                }
            }
            playlist.PlaylistEntries.Clear();
            foreach(var song in songs)
            {
                playlist.PlaylistEntries.Add(new WplPlaylistEntry()
                {
                    AlbumTitle = song.Album,
                    AlbumArtist = song.AlbumArtist,
                    Duration = song.Duration,
                    Path = song.Path,
                    TrackArtist = song.Artist,
                    TrackTitle = song.Title
                });
            }

            string updated = "";
            using(Stream stream = await file.OpenStreamForWriteAsync())//error unauthorized
            {
                updated = wplContent.Update(playlist, stream);
            }
            await FileIO.WriteTextAsync(file, updated);
        }
    }
}
