using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Common
{
    public class SongItemsFactory
    {
        public async Task<IEnumerable<SongItem>> GetSongItems(MusicItem item)
        {
            IEnumerable<SongItem> list = new List<SongItem>();
            var type = MusicItem.ParseType(item.GetParameter());
            switch (type)
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam, ((AlbumItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.albumartist:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumArtistAsync(((AlbumArtistItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.artist:
                    list = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = ApplicationSettingsHelper.ReadSettingsValue<bool>(SettingsKeys.IncludeSubFolders);
                    list = await DatabaseManager.Current.GetSongItemsFromFolderAsync(((FolderItem)item).Directory, subFolders);
                    break;
                case MusicItemTypes.genre:
                    list = await DatabaseManager.Current.GetSongItemsFromGenreAsync(((GenreItem)item).GenreParam);
                    break;
                case MusicItemTypes.plainplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.smartplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.song:
                    list = new List<SongItem>() { (SongItem)item };
                    break;
                case MusicItemTypes.radio:
                    list = new List<SongItem>() { (((RadioItem)item).ToSongItem()) };
                    break;
                case MusicItemTypes.onedrivefolder:
                case MusicItemTypes.dropboxfolder:
                case MusicItemTypes.pcloudfolder:
                    if (typeof(CloudRootFolder) == item.GetType())
                    {
                        var folder = (CloudRootFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        try
                        {
                            var id = await service.GetRootFolderId();
                            list = await service.GetSongItems(id);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        var folder = (CloudFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        try
                        {
                            list = await service.GetSongItems(folder.Id);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    break;
                default:
                    break;
            }
            return list;
        }

        public async Task<List<SongItem>> GetSongItems(IEnumerable<MusicItem> items)
        {
            List<SongItem> songs = new List<SongItem>();
            foreach(var item in items)
            {
                var s = await GetSongItems(item);
                songs.AddRange(s);
            }
            return songs;
        }
    }
}
