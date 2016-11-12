﻿using NextPlayerUWPDataLayer.Constants;
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
        public async Task<string> ToM3UContentAsync(PlaylistItem playlist, bool useRelativePaths, string folderPath)
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
            //if path are from different volumes don't use relative paths
            if (useRelativePaths)
            {
                char disk = ':';
                foreach (var song in songs)
                {
                    if (song.Path.Length > 1 && song.Path[1] == Path.VolumeSeparatorChar)
                    {
                        if (disk != ':' && song.Path[0] == disk)
                        {
                            useRelativePaths = false;
                            break;
                        }
                        disk = song.Path[0];
                    }
                }
            }
            if (useRelativePaths)
            {
                foreach (var song in songs)
                {
                    if (song.SourceType == Enums.MusicSource.LocalFile || song.SourceType == Enums.MusicSource.LocalNotMusicLibrary)
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
                    else if (song.SourceType == Enums.MusicSource.Dropbox || song.SourceType == Enums.MusicSource.OneDrive || song.SourceType == Enums.MusicSource.PCloud || song.SourceType == Enums.MusicSource.GoogleDrive)
                    {
                        
                    }
                }
            }
            else
            {
                foreach (var song in songs)
                {
                    if (song.SourceType == Enums.MusicSource.LocalFile || song.SourceType == Enums.MusicSource.LocalNotMusicLibrary)
                    {
                        sb.AppendLine(song.Path);
                    }
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

        public async Task AutoSavePlaylistAsync(PlaylistItem playlist)
        {
            bool autoSave = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists);
            if (!autoSave) return;
            

            StorageFolder playlistsFolder = await GetFolderWithAppPlaylistsAsync();
            if (playlistsFolder == null)
            {
                //clear imported table
                return;
            }
            if (!playlist.Path.StartsWith(playlistsFolder.Path))
            {
                //playlist was imported to app - no need to create backup
                return;
            }
            string newName = playlist.Name + ".m3u";

            string content = await ToM3UContentAsync(playlist, false, "");
            if (String.IsNullOrEmpty(playlist.Path))
            {
                try
                {
                    var file = await playlistsFolder.CreateFileAsync(newName, CreationCollisionOption.GenerateUniqueName);
                    newName = file.Name;
                    await FileIO.WriteTextAsync(file, content);
                    playlist.Path = file.Path;
                    var prop = await file.GetBasicPropertiesAsync();
                    playlist.DateModified = prop.DateModified.UtcDateTime;
                    await DatabaseManager.Current.UpdatePlainPlaylistAsync(playlist);
                }
                catch (Exception ex)
                {

                }
            }
            else if (playlist.Path.StartsWith(playlistsFolder.Path))
            {
                //update file
                try
                {
                    var file = await playlistsFolder.GetFileAsync(Path.GetFileName(playlist.Path));
                    await FileIO.WriteTextAsync(file, content);
                    var prop = await file.GetBasicPropertiesAsync();
                    playlist.DateModified = prop.DateModified.UtcDateTime;
                    await DatabaseManager.Current.UpdatePlainPlaylistAsync(playlist);
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

        public async Task ChangePlaylistNameAsync(PlaylistItem playlist)
        {
            bool autoSave = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists);
            if (!autoSave) return;
            if (String.IsNullOrEmpty(playlist.Path))
            {
                await AutoSavePlaylistAsync(playlist);
            }
            else
            {
                string oldFileName = Path.GetFileName(playlist.Path);
                string newFileName = playlist.Name + ".m3u";

                StorageFolder playlistsFolder = await GetFolderWithAppPlaylistsAsync();
                if (playlistsFolder == null)
                {
                    //log
                    //clear imported table
                    return;
                }
                try
                {
                    var file = await playlistsFolder.GetFileAsync(oldFileName);
                    await file.RenameAsync(newFileName, NameCollisionOption.GenerateUniqueName);
                    newFileName = file.Name;
                    playlist.Path = file.Path;
                    var prop = await file.GetBasicPropertiesAsync();
                    playlist.DateModified = prop.DateModified.UtcDateTime;
                    await DatabaseManager.Current.UpdatePlainPlaylistAsync(playlist);
                }
                catch (FileNotFoundException)
                {
                    await AutoSavePlaylistAsync(playlist);
                }
                catch (Exception ex)
                {
                    //log
                }
            }
        }

        /// <summary>
        /// Delete playlist and playlist entries in db and delete playlist file if file is in app playlist folder
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public async Task DeletePlaylistAsync(PlaylistItem playlist)
        {
            if (!playlist.IsSmart)
            {
                if (!String.IsNullOrEmpty(playlist.Path))
                {
                    StorageFolder playlistsFolder = await GetFolderWithAppPlaylistsAsync();
                    if (playlistsFolder != null && playlist.Path.StartsWith(playlistsFolder.Path))
                    {
                        try
                        {
                            var file = await playlistsFolder.GetFileAsync(Path.GetFileName(playlist.Path));
                            await file.DeleteAsync();
                        }
                        catch (Exception ex)
                        {
                            //log
                        }
                    }
                }
                await DatabaseManager.Current.DeletePlainPlaylistAsync(playlist.Id);
            }
            else if (playlist.IsSmartAndNotDefault)
            {
                await DatabaseManager.Current.DeleteSmartPlaylistAsync(playlist.Id);
            }
        }

        public async Task ExportPlaylistToM3UAsync(PlaylistItem playlist, StorageFile file, bool useRealtivePaths = false)
        {
            string folderPath = Path.GetDirectoryName(file.Path);
            PlaylistExporter pe = new PlaylistExporter();
            string content = await pe.ToM3UContentAsync(playlist, useRealtivePaths, folderPath);
            await FileIO.WriteTextAsync(file, content);
            playlist.Path = file.Path;
            var prop = await file.GetBasicPropertiesAsync();
            playlist.DateModified = prop.DateModified.UtcDateTime;
        }

        public async Task<StorageFolder> GetFolderWithAppPlaylistsAsync()
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

        public async Task SavePlainPlaylistsInPlaylistsFolderAsync()
        {
            var playlists = await DatabaseManager.Current.GetPlainPlaylistsAsync();
            foreach(var playlist in playlists)
            {
                await AutoSavePlaylistAsync(playlist);
            }
        }

        public async Task DeleteAllPlainPlaylistsAsync()
        {
            var playlists = await DatabaseManager.Current.GetPlainPlaylistsAsync();
            foreach (var playlist in playlists)
            {
                await DeletePlaylistAsync(playlist);
            }
        }
    }
}
