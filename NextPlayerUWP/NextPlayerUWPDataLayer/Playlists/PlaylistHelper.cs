using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Playlists.ContentCreator;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists
{
    public enum PlaylistType
    {
        m3u,
        m3u8,
        pls,
        wpl,
        zpl,
        unknown
    }

    public class PlaylistHelper
    {
        public PlaylistHelper()
        {
        }

        public async Task ExportPlaylistToFile(PlaylistItem item, StorageFile file, bool hide = false, bool useRelativePaths = false)
        {
            IEnumerable<SongItem> songs = await GetSongsFromPlaylist(item);
            if (useRelativePaths)
            {
                string folderPath = Path.GetDirectoryName(file.Path);
                foreach(var song in songs)
                {
                    song.Path = PlaylistsNET.Utils.Utils.MakeRelativePath(folderPath, song.Path);
                }
            }

            string content = PrepareContent(item, songs, FileTypeToPlaylistType(file.FileType));

            bool saved = await SaveToFile(file, content);
            if (saved)
            {
                if (item.IsSmart)
                {
                    int id = DatabaseManager.Current.InsertPlainPlaylist(file.Name);
                    var p = await DatabaseManager.Current.GetPlainPlaylistAsync(id);
                    p.Path = file.Path;
                    var properties = await file.GetBasicPropertiesAsync();
                    p.DateModified = properties.DateModified.UtcDateTime;
                    p.IsHidden = false;
                    await DatabaseManager.Current.UpdatePlainPlaylistAsync(p);
                    await DatabaseManager.Current.AddToPlaylist(id, songs);
                }
                else if (String.IsNullOrEmpty(item.Path)) // if playlist was created in app and is exported first time
                {
                    var properties = await file.GetBasicPropertiesAsync();
                    item.DateModified = properties.DateModified.UtcDateTime;
                    item.Path = file.Path;
                    await DatabaseManager.Current.UpdatePlainPlaylistAsync(item);
                    await DeleteBackupPlaylistAsync(item);
                }

                await FutureAccessHelper.AddToFutureAccessListAndSaveTokenAsync(file);
            }
        }

        public async Task UpdatePlaylistFile(PlaylistItem item)
        {
            if (!item.IsSmart)
            {                
                IEnumerable<SongItem> songs = await GetSongsFromPlaylist(item);
                if (String.IsNullOrEmpty(item.Path))
                {
                    IContentCreator contentCreator = GetContentCreator(PlaylistType.m3u);
                    string content = contentCreator.CreateContent(item, songs);
                    await CreateOrReplaceBackupPlaylist(item, content);
                }
                else
                {
                    PlaylistType type = FileTypeToPlaylistType(Path.GetExtension(item.Path));
                    IContentCreator contentCreator = GetContentCreator(type);
                    StorageFile file = null;
                    try
                    {
                        file = await StorageFile.GetFileFromPathAsync(item.Path);
                    }
                    catch(Exception ex)
                    {
                        file = await FutureAccessHelper.GetFileFromPathAsync(item.Path);
                    }
               
                    if (file != null)
                    {

                        await contentCreator.UpdateContent(item, songs, file);
                        var properties = await file.GetBasicPropertiesAsync();
                        item.DateModified = properties.DateModified.UtcDateTime;
                        await DatabaseManager.Current.UpdatePlainPlaylistAsync(item);
                    }
                    else
                    {
                        contentCreator = GetContentCreator(PlaylistType.m3u);
                        string content = contentCreator.CreateContent(item, songs);
                        await CreateOrReplaceBackupPlaylist(item, content);
                    }
                }
            }
        }

        public async Task EditName(PlaylistItem item, string newName)
        {
            string oldName = item.Name;
            item.Name = newName;
            if (!item.IsSmart)
            {
                await DatabaseManager.Current.UpdatePlainPlaylistAsync(item);
                if (String.IsNullOrEmpty(item.Path))
                {
                    await RenameBackupPlaylist(item.Id, oldName, newName);
                }
                if (!String.IsNullOrEmpty(item.Path) && item.Path.EndsWith(".wpl") || item.Path.EndsWith(".zpl"))
                {
                    await UpdatePlaylistFile(item);
                }
            }
            else
            {
                await DatabaseManager.Current.UpdateSmartPlaylist(item.Id, item.Name, item.IsHidden);
            }
        }

        public async Task EditPlaylist(PlaylistItem item, bool hide)
        {
            item.IsHidden = hide;
            if (!item.IsSmart)
            {
                await DatabaseManager.Current.UpdatePlainPlaylistAsync(item);
            }
            else
            {
                await DatabaseManager.Current.UpdateSmartPlaylist(item.Id, item.Name, item.IsHidden);
            }
        }

        public async Task<IEnumerable<SongItem>> GetSongsFromPlaylist(PlaylistItem item)
        {
            IEnumerable<SongItem> songs;
            if (item.IsSmart)
            {
                songs = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(item.Id);
            }
            else
            {
                songs = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(item.Id);
            }
            return songs;
        }

        public async Task DeleteFile(PlaylistItem playlist)
        {
            if (playlist.IsSmart || String.IsNullOrEmpty(playlist.Path)) return;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(playlist.Path);
                await file.DeleteAsync();
                playlist.Path = "";
                await DatabaseManager.Current.UpdatePlainPlaylistAsync(playlist);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task DeletePlaylistItem(PlaylistItem playlist)
        {
            if (!playlist.IsNotDefault) return;
            if (playlist.IsSmart)
            {
                await DatabaseManager.Current.DeleteSmartPlaylistAsync(playlist.Id);
            }
            else
            {
                await DatabaseManager.Current.DeletePlainPlaylistAsync(playlist.Id);
                await DeleteBackupPlaylistAsync(playlist);
            }
        }

        private async Task<bool> SaveToFile(StorageFile file, string content)
        {
            try
            {
                await FileIO.WriteTextAsync(file, content);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private string PrepareContent(PlaylistItem item, IEnumerable<SongItem> songs, PlaylistType type)
        {
            string content = "";
            IContentCreator contentCreator = GetContentCreator(type);
            content = contentCreator.CreateContent(item, songs);
            return content;
        }

        private PlaylistType FileTypeToPlaylistType(string type)
        {
            PlaylistType pType;
            type = type.ToLower();
            switch (type)
            {
                case ".m3u":
                    pType = PlaylistType.m3u;
                    break;
                case ".m3u8":
                    pType = PlaylistType.m3u8;
                    break;
                case ".pls":
                    pType = PlaylistType.pls;
                    break;
                case ".wpl":
                    pType = PlaylistType.wpl;
                    break;
                case ".zpl":
                    pType = PlaylistType.zpl;
                    break;
                default:
                    pType = PlaylistType.unknown;
                    break;
            }
            return pType;
        }

        private async Task CreateOrReplaceBackupPlaylist(PlaylistItem item, string content)
        {
            if (!item.IsSmart)
            {
                StorageFolder playlistsFolder = await GetFolderWithAppPlaylistsAsync();
                if (playlistsFolder != null)
                {
                    try
                    {
                        string name = GenerateFilename(item);
                        var file = await playlistsFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(file, content);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private async Task RenameBackupPlaylist(int id, string oldPlaylistName, string newPlaylistName)
        {
            StorageFolder playlistsFolder = await GetFolderWithAppPlaylistsAsync();
            if (playlistsFolder != null)
            {
                try
                {
                    string oldFilename = GenerateFilename(id, oldPlaylistName);
                    string newFilename = GenerateFilename(id, newPlaylistName);
                    var file = await playlistsFolder.GetFileAsync(oldFilename);
                    await file.RenameAsync(newFilename, NameCollisionOption.ReplaceExisting);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task DeleteBackupPlaylistAsync(PlaylistItem playlist)
        {
            if (!playlist.IsSmart)
            {
                StorageFolder playlistsFolder = await GetFolderWithAppPlaylistsAsync();
                if (playlistsFolder != null)
                {
                    try
                    {
                        string filename = GenerateFilename(playlist);
                        var file = await playlistsFolder.GetFileAsync(filename);
                        await file.DeleteAsync();
                    }
                    catch (Exception ex)
                    {
                        //log
                    }
                }
            }
        }       

        private string GenerateFilename(PlaylistItem playlist)
        {
            return playlist.Id + "-" + playlist.Name;
        }

        private string GenerateFilename(int id, string playlistName)
        {
            return id + "-" + playlistName;
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
            catch (Exception ex)
            {
                return null;
            }
        }

        private IContentCreator GetContentCreator(PlaylistType type)
        {
            IContentCreator contentCreator;
            switch (type)
            {
                case PlaylistType.m3u:
                    contentCreator = new M3uContentCreator();
                    break;
                case PlaylistType.pls:
                    contentCreator = new PlsContentCreator();
                    break;
                case PlaylistType.wpl:
                    contentCreator = new WplContentCreator();
                    break;
                case PlaylistType.zpl:
                    contentCreator = new ZplContentCreator();
                    break;
                default:
                    contentCreator = new EmptyContentCreator();
                    break;
            }
            return contentCreator;
        }
    }
}
