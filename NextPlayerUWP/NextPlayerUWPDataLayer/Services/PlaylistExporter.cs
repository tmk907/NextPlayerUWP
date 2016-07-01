using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

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
                foreach (var song in songs)
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
                foreach (var song in songs)
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

        public async Task AutoSavePlaylist(PlaylistItem playlist)
        {
            bool autoSave = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists);
            if (!autoSave) return;
            string newName = playlist.Name + ".m3u";

            StorageFolder playlistsFolder = await GetPlaylistsFolder();
            if (playlistsFolder == null)
            {
                //clear imported table
                return;
            }
            string content = await ExportAsM3U(playlist, false, "");
            var list = await DatabaseManager.Current.GetImportedPlaylists();
            var imported = list.SingleOrDefault(p => p.PlainPlaylistId.Equals(playlist.Id));
            if (imported == null)
            {
                var file = await playlistsFolder.CreateFileAsync(newName, CreationCollisionOption.GenerateUniqueName);
                newName = file.Name;
                await FileIO.WriteTextAsync(file, content);
                DatabaseManager.Current.InsertImportedPlaylist(newName, file.Path, playlist.Id);
            }
            else if (imported.Path.StartsWith(playlistsFolder.Path))
            {
                try
                {
                    var file = await playlistsFolder.GetFileAsync(Path.GetFileName(imported.Path));
                    await FileIO.WriteTextAsync(file, content);
                }
                catch (FileNotFoundException)
                {
                    //await AutoSavePlaylist(playlist);??
                }
                catch (Exception ex)
                {
                    //log
                }
            }
        }

        public async Task ChangePlaylistName(PlaylistItem playlist)
        {
            bool autoSave = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists);
            if (!autoSave) return;
            var list = await DatabaseManager.Current.GetImportedPlaylists();
            var imported = list.SingleOrDefault(p => p.PlainPlaylistId.Equals(playlist.Id));
            if (imported == null)
            {
                await AutoSavePlaylist(playlist);
            }
            else
            {
                string oldFileName = Path.GetFileName(imported.Path);
                string newFileName = playlist.Name + ".m3u";

                StorageFolder playlistsFolder = await GetPlaylistsFolder();
                if (playlistsFolder == null)
                {
                    //clear imported table
                    return;
                }
                try
                {
                    var file = await playlistsFolder.GetFileAsync(oldFileName);
                    await file.RenameAsync(newFileName, NameCollisionOption.GenerateUniqueName);
                    newFileName = file.Name;
                    imported.Path = imported.Path.Replace(oldFileName, newFileName);
                    imported.Name = playlist.Name;
                    await DatabaseManager.Current.UpdateImportedPlaylist(imported);
                }
                catch (FileNotFoundException)
                {
                    await AutoSavePlaylist(playlist);
                }
                catch (Exception ex)
                {
                    //log
                }
                
            }
        }

        public async Task DeletePlaylist(PlaylistItem playlist)
        {
            bool autoSave = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists);
            //if (!autoSave) return;
            var list = await DatabaseManager.Current.GetImportedPlaylists();
            var imported = list.SingleOrDefault(p => p.PlainPlaylistId.Equals(playlist.Id));
            if (imported != null)
            {
                StorageFolder playlistsFolder = await GetPlaylistsFolder();
                if (playlistsFolder != null && imported.Path.StartsWith(playlistsFolder.Path))
                {
                    try
                    {
                        var file = await playlistsFolder.GetFileAsync(Path.GetFileName(imported.Path));
                        await file.DeleteAsync();
                    }
                    catch (Exception ex)
                    {
                        //log
                    }
                    await DatabaseManager.Current.DeleteImportedPlaylist(imported);
                }
            }
        }

        private async Task<StorageFolder> GetPlaylistsFolder()
        {
            try
            {
                var playlistsFolder = await KnownFolders.Playlists.GetFolderAsync("Next-Player");
                return playlistsFolder;
            }
            catch (FileNotFoundException)
            {
                try
                {
                    var playlistsFolder = await KnownFolders.Playlists.CreateFolderAsync("Next-Player");
                    return playlistsFolder;
                }
                catch (Exception)
                {
                    //log
                    return null;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public async Task SaveAllPlaylists()
        {
            var playlists = await DatabaseManager.Current.GetPlainPlaylistsAsync();
            foreach(var playlist in playlists)
            {
                await AutoSavePlaylist(playlist);
            }
        }

        public async Task DeleteAllPlaylists()
        {
            var playlists = await DatabaseManager.Current.GetPlainPlaylistsAsync();
            foreach (var playlist in playlists)
            {
                await DeletePlaylist(playlist);
            }
        }
    }
}
