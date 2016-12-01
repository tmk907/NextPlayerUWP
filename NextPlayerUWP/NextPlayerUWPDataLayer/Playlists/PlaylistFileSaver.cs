using PlaylistsNET.Content;
using PlaylistsNET.Model;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class PlaylistFileSaver
    {
        public async Task SaveM3uToFile(StorageFile file, M3uPlaylist playlist)
        {
            M3uContent content = new M3uContent();
            await FileIO.WriteTextAsync(file, content.Create(playlist));
        }

        public async Task SaveM3u8ToFile(StorageFile file, M3uPlaylist playlist)
        {
            M3u8Content content = new M3u8Content();
            await FileIO.WriteTextAsync(file, content.Create(playlist), Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }

        public async Task SaveToPlsFile(StorageFile file, PlsPlaylist playlist)
        {
            PlsContent content = new PlsContent();
            await FileIO.WriteTextAsync(file, content.Create(playlist));
        }

        public async Task SaveToWplFile(StorageFile file, WplPlaylist playlist)
        {
            WplContent content = new WplContent();
            await FileIO.WriteTextAsync(file, content.Create(playlist));
        }

        public async Task SaveToZplFile(StorageFile file, ZplPlaylist playlist)
        {
            ZplContent content = new ZplContent();
            await FileIO.WriteTextAsync(file, content.Create(playlist));
        }
    }
}
