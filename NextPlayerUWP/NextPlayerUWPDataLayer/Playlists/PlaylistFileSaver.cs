using PlaylistsNET.Content;
using PlaylistsNET.Models;
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
            await FileIO.WriteTextAsync(file, content.ToText(playlist));
        }

        public async Task SaveM3u8ToFile(StorageFile file, M3uPlaylist playlist)
        {
            M3uContent content = new M3uContent();
            await FileIO.WriteTextAsync(file, content.ToText(playlist), Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }

        public async Task SaveToPlsFile(StorageFile file, PlsPlaylist playlist)
        {
            PlsContent content = new PlsContent();
            await FileIO.WriteTextAsync(file, content.ToText(playlist));
        }

        public async Task SaveToWplFile(StorageFile file, WplPlaylist playlist)
        {
            WplContent content = new WplContent();
            await FileIO.WriteTextAsync(file, content.ToText(playlist));
        }

        public async Task SaveToZplFile(StorageFile file, ZplPlaylist playlist)
        {
            ZplContent content = new ZplContent();
            await FileIO.WriteTextAsync(file, content.ToText(playlist));
        }
    }
}
