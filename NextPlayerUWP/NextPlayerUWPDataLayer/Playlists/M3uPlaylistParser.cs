using NextPlayerUWPDataLayer.Model;
using Windows.Storage;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class M3uPlaylistParser : BasePlaylistParser
    {
        public override async Task<ImportedPlaylist> ParsePlaylist(StorageFile file)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.Name = file.Name;
            iplaylist.Path = file.Path;
            var prop = await file.GetBasicPropertiesAsync();
            DateTime dateModified = prop.DateModified.UtcDateTime;
            iplaylist.DateModified = dateModified;
            string folderPath = Path.GetDirectoryName(file.Path);
            using (var stream = await file.OpenStreamForReadAsync())
            {
                StreamReader streamReader;
                if (file.FileType.ToLower() == ".m3u8")
                {
                    streamReader = new StreamReader(stream, Encoding.UTF8);
                }
                else
                {
                    streamReader = new StreamReader(stream);
                }
                 
                bool isExtended = false;
                bool isFirstLine = true;
                string title = "";
                bool prevLineIsExtInf = false;
                string displayName = "";

                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (isFirstLine && line.StartsWith("#EXTM3U"))
                    {
                        isExtended = true;
                    }
                    else if (isExtended && line.StartsWith("#EXTINF"))
                    {
                        prevLineIsExtInf = true;
                        try
                        {
                            title = line.Substring(line.IndexOf(',') + 1);
                        }
                        catch
                        {
                            title = "";
                        }
                    }
                    else if (line.StartsWith(@"http://") || line.StartsWith(@"https://"))
                    {
                        displayName = (prevLineIsExtInf && title != "") ? title : line;
                        prevLineIsExtInf = false;
                        iplaylist.SongPaths.Add(new ImportedPlaylist.Song()
                        {
                            DisplayName = displayName,
                            Path = line
                        });
                    }
                    else
                    {
                        if (prevLineIsExtInf && title != "")
                        {
                            displayName = title;                          
                        }
                        else
                        {
                            try
                            {
                                displayName = Path.GetFileName(line);
                            }
                            catch
                            { 
                                displayName = line;
                            }
                        }
                        prevLineIsExtInf = false;
                        iplaylist.SongPaths.Add(new ImportedPlaylist.Song()
                        {
                            DisplayName = displayName,
                            Path = CreateFullFilePath(line, folderPath)
                        });
                    }
                    isFirstLine = false;
                }
            }
            return iplaylist;
        }        
    }
}
