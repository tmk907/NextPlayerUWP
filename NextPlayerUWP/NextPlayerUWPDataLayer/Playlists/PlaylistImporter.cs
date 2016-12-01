using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class PlaylistImporter
    {
        //public async Task<List<MaxPlaylistEntry>> getmax(StorageFile file)
        //{
        //    string type = file.FileType.ToLower();

        //    List<MaxPlaylistEntry> list = new List<MaxPlaylistEntry>();
        //    PlaylistFileReader pr = new PlaylistFileReader();
        //    if (type == ".m3u" || type == ".m3u8")
        //    {
        //        var p = await pr.OpenM3uPlaylist(file);
        //        foreach (var entry in p.PlaylistEntries)
        //        {
                    
        //        }
        //    }
        //    else if (type == ".wpl")
        //    {
        //        parser = new WplPlaylistParser();
        //    }
        //    else if (type == ".pls")
        //    {
        //        parser = new PlsPlaylistParser();
        //    }
        //    else if (type == ".zpl")
        //    {
        //        parser = new ZplPlaylistParser();
        //    }
        //    return list;
        //}

        public async Task<ImportedPlaylist> Import(StorageFile file)
        {
            string type = file.FileType.ToLower();
            var prop = await file.GetBasicPropertiesAsync();
            ImportedPlaylist newPlaylist = new ImportedPlaylist();
            newPlaylist.DateModified = prop.DateModified.UtcDateTime;
            newPlaylist.Path = file.Path;
            newPlaylist.Name = file.Name;

            PlaylistFileReader pr = new PlaylistFileReader();
            if (type == ".m3u" || type == ".m3u8")
            {
                var p = await pr.OpenM3uPlaylist(file);
                foreach (var entry in p.PlaylistEntries)
                {
                    MaxPlaylistEntry m = new MaxPlaylistEntry(entry);
                    newPlaylist.max.Add(m);
                }
            }
            else if (type == ".wpl")
            {
                var p = await pr.OpenWplPlaylist(file);
                foreach (var entry in p.PlaylistEntries)
                {
                    MaxPlaylistEntry m = new MaxPlaylistEntry(entry);
                    newPlaylist.max.Add(m);
                }
            }
            else if (type == ".pls")
            {
                var p = await pr.OpenPlsPlaylist(file);
                foreach (var entry in p.PlaylistEntries)
                {
                    MaxPlaylistEntry m = new MaxPlaylistEntry(entry);
                    newPlaylist.max.Add(m);
                }
            }
            else if (type == ".zpl")
            {
                var p = await pr.OpenZplPlaylist(file);
                foreach (var entry in p.PlaylistEntries)
                {
                    MaxPlaylistEntry m = new MaxPlaylistEntry(entry);
                    newPlaylist.max.Add(m);
                }
            }

            return newPlaylist;
        }
    }
}
