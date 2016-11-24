using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class PlsPlaylistParser : BasePlaylistParser
    {
        private string folderPath;

        public override async Task<ImportedPlaylist> ParsePlaylist(StorageFile file)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.Name = file.Name;
            iplaylist.Path = file.Path;
            var prop = await file.GetBasicPropertiesAsync();
            DateTime dateModified = prop.DateModified.UtcDateTime;
            iplaylist.DateModified = dateModified;
            folderPath = Path.GetDirectoryName(file.Path);
            using (var stream = await file.OpenStreamForReadAsync())
            {
                StreamReader streamReader = new StreamReader(stream);
                if (!streamReader.EndOfStream)
                {
                    string header = streamReader.ReadLine();
                    if (header != "[playlist]")
                    {
                        return iplaylist;
                    }
                }
                int prevNr = -1;
                bool prevIsFile = false;
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    int nr = GetNr(line);
                    if (line.StartsWith("File"))
                    {
                        var song = ParseFileEntry(line);
                        iplaylist.SongPaths.Add(song);
                        prevIsFile = true;
                    }
                    else if (line.StartsWith("Title"))
                    {
                        string title = ParseTitleEntry(line);
                        if (!String.IsNullOrEmpty(title) && prevIsFile && nr == prevNr && iplaylist.SongPaths.Count > 0)
                        {
                            iplaylist.SongPaths.LastOrDefault().DisplayName = title;
                        }
                        prevIsFile = false;
                    }
                    else
                    {
                        prevIsFile = false;
                    }
                    prevNr = nr;
                }
            }
            return iplaylist;
        }

        private int GetNr(string line)
        {
            int nr = -1;
            if (line.StartsWith("File"))
            {
                try
                {
                    //0123456
                    //File1=
                    //File10=
                    nr = Int32.Parse(line.Substring(4, line.IndexOf('=') - 4));
                }
                catch { }
            }
            else if (line.StartsWith("Title"))
            {
                try
                {
                    //01234567
                    //Title1=
                    //Title10=
                    nr = Int32.Parse(line.Substring(5, line.IndexOf('=') - 5));
                }
                catch { }
            }
            else if (line.StartsWith("Length"))
            {
                try
                {
                    //012345678
                    //Length1=
                    //Length10=
                    nr = Int32.Parse(line.Substring(6, line.IndexOf('=') - 6));
                }
                catch { }
            }
            return nr;
        }

        private ImportedPlaylist.Song ParseFileEntry(string line)
        {
            ImportedPlaylist.Song song = null;
            try
            {
                string path = line.Substring(line.IndexOf('=') + 1);
                if (path.StartsWith(@"http://") || path.StartsWith(@"https://"))
                {
                    song = new ImportedPlaylist.Song
                    {
                        DisplayName = path,
                        Path = path
                    };
                }
                else
                {
                    string displayName = Path.GetFileName(path);
                    song = new ImportedPlaylist.Song()
                    {
                        DisplayName = displayName,
                        Path = CreateFullFilePath(path, folderPath)
                    };
                }
            }
            catch (Exception)
            {

            }
            return song;
        }

        private string ParseTitleEntry(string line)
        {
            string title = null;
            try
            {
                title = line.Substring(line.IndexOf('=') + 1);
            }
            catch { }
            return title;
        }

        private int ParseLengthEntry(string line)
        {
            int length = -1;
            try
            {
                length = Int32.Parse(line.Substring(line.IndexOf('=') + 1));
            }
            catch { }
            return length;
        }
    }
}
