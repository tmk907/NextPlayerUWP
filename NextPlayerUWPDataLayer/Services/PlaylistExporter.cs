using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Services
{
    public class PlaylistExporter
    {
        public async Task<string> ExportAsM3U(PlaylistItem playlist, bool relativePaths, string folderPath)
        {
            ObservableCollection<SongItem> songs;
            if (playlist.IsSmart)
            {
                songs = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(playlist.Id);
            }
            else
            {
                songs = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(playlist.Id);
            }
            string content = "";
            int capacity = (songs.Count != 0) ? songs.Count * songs.FirstOrDefault().Path.Length : 0;
            StringBuilder sb = new StringBuilder(capacity);
            if (!folderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folderPath += Path.DirectorySeparatorChar;
            }
            if (relativePaths)
            {
                foreach(var song in songs)
                {
                    string p = MakeRelativePath(folderPath, song.Path);
                    if (!String.IsNullOrEmpty(p))
                    {
                        if (!p.StartsWith("..") && !p.StartsWith(Path.DirectorySeparatorChar.ToString()) && p.Contains(Path.DirectorySeparatorChar))
                        {
                            sb.Append(Path.DirectorySeparatorChar);
                        }
                        sb.AppendLine(p);
                    }
                }
            }
            else
            {
                foreach(var song in songs)
                {
                    sb.AppendLine(song.Path);
                }
            }

            content = sb.ToString();

            return content;
        }

        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.CurrentCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
