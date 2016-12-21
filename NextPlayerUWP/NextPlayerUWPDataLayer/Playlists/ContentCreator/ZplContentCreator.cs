﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using Windows.Storage;
using PlaylistsNET.Model;
using PlaylistsNET.Content;

namespace NextPlayerUWPDataLayer.Playlists.ContentCreator
{
    public class ZplContentCreator : IContentCreator
    {
        public string CreateContent(PlaylistItem item, IEnumerable<SongItem> songs)
        {
            var playlist = new ZplPlaylist();
            playlist.Title = item.Name;
            foreach (var song in songs)
            {
                playlist.PlaylistEntries.Add(new ZplPlaylistEntry()
                {
                    AlbumTitle = song.Album,
                    AlbumArtist = song.AlbumArtist,
                    Duration = song.Duration,
                    Path = song.Path,
                    TrackArtist = song.Artist,
                    TrackTitle = song.Title
                });
            }
            var creator = new ZplContent();
            string content = creator.Create(playlist);
            return content;
        }

        public async Task UpdateContent(PlaylistItem item, IEnumerable<SongItem> songs, StorageFile file)
        {
            var zplContent = new ZplContent();
            PlaylistFileReader pr = new PlaylistFileReader();
            var playlist = await pr.OpenZplPlaylist(file);
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
            foreach (var song in songs)
            {
                playlist.PlaylistEntries.Add(new ZplPlaylistEntry()
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
            using (Stream stream = await file.OpenStreamForWriteAsync())
            {
                updated = zplContent.Update(playlist, stream);
            }
            await FileIO.WriteTextAsync(file, updated);
        }
    }
}