using NextPlayerUWPDataLayer.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists
{
    public abstract class BasePlaylistParser : IPlaylistParser
    {
        public abstract Task<ImportedPlaylist> ParsePlaylist(StorageFile file);

        protected static string CreateFullFilePath(string filePath, string folderPath)
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
