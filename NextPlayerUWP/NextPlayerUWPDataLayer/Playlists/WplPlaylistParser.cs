using System;
using System.Linq;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using Windows.Storage;
using System.IO;
using System.Xml.Linq;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class WplPlaylistParser : BasePlaylistParser
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
                try
                {
                    var doc = XDocument.Load(stream);
                    var mediaElements = doc.Descendants("body").Elements("seq").Elements("media");
                    foreach (var media in mediaElements)
                    {
                        var src = media.Attribute("src").Value;
                        var trackTitle = media.Attribute("trackTitle")?.Value;
                        string displayName = "";
                        if (!String.IsNullOrEmpty(trackTitle))
                        {
                            displayName = trackTitle;
                        }
                        else
                        {
                            displayName = Path.GetFileName(src);
                        }
                        iplaylist.SongPaths.Add(new ImportedPlaylist.Song()
                        {
                            DisplayName = displayName,
                            Path = CreateFullFilePath(src, folderPath)
                        });
                    }
                    string title = doc.Descendants("head").Elements("title").FirstOrDefault()?.Value;
                    if (!String.IsNullOrEmpty(title))
                    {
                        iplaylist.Name = title;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return iplaylist;
        }
    }
}
