using NextPlayerUWPDataLayer.Model;
using PlaylistsNET.Content;
using PlaylistsNET.Model;
using System.Collections.Generic;
using System;
using Windows.Storage;
using System.Threading.Tasks;
using System.Linq;

namespace NextPlayerUWPDataLayer.Playlists.ContentCreator
{
    public class M3uContentCreator : IContentCreator
    {
        public bool IsExtended { get; set; }
        public bool UseExtraInfo { get; set; }

        public M3uContentCreator()
        {
            IsExtended = true;
            UseExtraInfo = false;
        }

        public string CreateContent(PlaylistItem item, IEnumerable<SongItem> songs)
        {
            var playlist = new M3uPlaylist();
            playlist.IsExtended = IsExtended;
            foreach (var song in songs)
            {
                playlist.PlaylistEntries.Add(new M3uPlaylistEntry()
                {
                    Album = (UseExtraInfo) ? song.Album : null,
                    AlbumArtist = (UseExtraInfo) ? song.AlbumArtist : null,
                    Duration = song.Duration,
                    Path = song.Path,
                    Title = song.Title
                });
            }
            var creator = new M3uContent();
            string content = creator.Create(playlist);
            return content;
        }

        public async Task UpdateContent(PlaylistItem item, IEnumerable<SongItem> songs, StorageFile file)
        {
            PlaylistFileReader pr = new PlaylistFileReader();
            var playlist = await pr.OpenM3uPlaylist(file);
            if (playlist.PlaylistEntries.Count > 0)
            {
                if (playlist.PlaylistEntries.FirstOrDefault(e => e.Path.Contains(System.IO.Path.VolumeSeparatorChar)) == null)
                {
                    string folderPath = System.IO.Path.GetDirectoryName(file.Path);
                    foreach(var song in songs)
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
