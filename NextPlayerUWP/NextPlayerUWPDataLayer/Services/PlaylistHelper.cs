using NextPlayerUWPDataLayer.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Services
{
    public class PlaylistHelper
    {
        public async Task<ImportedPlaylist> ParseM3UPlaylist(StorageFile file)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.Name = file.DisplayName;
            iplaylist.Path = file.Path;
            string folderPath = Path.GetDirectoryName(file.Path);
            using (var stream = await file.OpenStreamForReadAsync())
            {
                StreamReader streamReader = new StreamReader(stream);
                bool isExtended = false;
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line.StartsWith("#"))
                    {
                        if (line.StartsWith("#EXTM3U"))
                        {
                            isExtended = true;
                        }
                        else if (line.StartsWith("#EXTINF"))
                        {

                        }
                    }
                    else if (line.StartsWith("http://") || line.StartsWith("https://"))
                    {
                        iplaylist.SongPaths.Add(line);
                    }
                    else
                    {
                        iplaylist.SongPaths.Add(CreateFullFilePath(line, folderPath));
                    }
                }
            }
            return iplaylist;
        }

        public async Task<ImportedPlaylist> ParseWPLPlaylist(StorageFile file)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.Name = file.DisplayName;
            iplaylist.Path = file.Path;
            string folderPath = Path.GetDirectoryName(file.Path);
            using (var stream = await file.OpenStreamForReadAsync())
            {
                try
                {
                    var doc = XDocument.Load(stream).Descendants("body").Elements("seq").Elements("media");
                    foreach (var media in doc)
                    {
                        var src = media.Attribute("src").Value;
                        iplaylist.SongPaths.Add(CreateFullFilePath(src, folderPath));
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return iplaylist;
        }

        public async Task<ImportedPlaylist> ParsePLSPlaylist(StorageFile file)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.Name = file.DisplayName;
            iplaylist.Path = file.Path;
            string folderPath = Path.GetDirectoryName(file.Path);
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
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line.StartsWith("File"))
                    {
                        string path = line.Substring(line.IndexOf('=') + 1);
                        if (path.StartsWith("http"))
                        {
                            iplaylist.SongPaths.Add(path);
                        }
                        else
                        {
                            iplaylist.SongPaths.Add(CreateFullFilePath(path, folderPath));
                        }
                    }

                }
            }
            return iplaylist;
        }

        private static string CreateFullFilePath(string filePath, string folderPath)
        {
            string fullpath = "";
            if (filePath.StartsWith(@"\"))
            {
                fullpath = folderPath + filePath;
            }
            else
            {
                bool isRooted = false;
                try
                {
                    isRooted = Path.IsPathRooted(filePath);
                }
                catch (Exception)
                { }
                if (isRooted)
                {
                    fullpath = filePath;
                }
                else if (filePath.StartsWith(@"..\"))
                {
                    try
                    {
                        fullpath = Path.GetFullPath(folderPath + @"\" + filePath);
                    }
                    catch (Exception)
                    { }
                }
                else
                {
                    fullpath = folderPath + @"\" + filePath;
                }
            }
            return fullpath;
        }
    }
}
