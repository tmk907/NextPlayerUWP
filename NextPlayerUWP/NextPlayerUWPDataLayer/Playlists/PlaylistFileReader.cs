using PlaylistsNET.Content;
using PlaylistsNET.Model;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class PlaylistFileReader
    {
        public async Task<IBasePlaylist<BasePlaylistEntry>> OpenPlaylist(StorageFile file)
        {
            IBasePlaylist<BasePlaylistEntry> playlist;

            string type = file.FileType;
            switch (type)
            {
                case ".m3u":
                    playlist = await OpenM3uPlaylist(file);
                    break;
                case ".m3u8":
                    playlist = await OpenM3u8Playlist(file);
                    break;
                case ".pls":
                    playlist = await OpenPlsPlaylist(file);
                    break;
                case ".wpl":
                    playlist = await OpenWplPlaylist(file);
                    break;
                case ".zpl":
                    playlist = await OpenZplPlaylist(file);
                    break;
                default:
                    playlist = new BasePlaylist<BasePlaylistEntry>();
                    break;
            }

            return playlist;
        }

        public async Task<IBasePlaylist<BasePlaylistEntry>> OpenPlaylist2(StorageFile file)
        {
            IBasePlaylist<BasePlaylistEntry> playlist;
            string type = file.FileType;
            PlaylistContentFactory factory = new PlaylistContentFactory();
            IPlaylistContentReader<IBasePlaylist<BasePlaylistEntry>> content = factory.GetPlaylistContentReader(file.FileType);
            using(var stream = await file.OpenStreamForReadAsync())
            {
                playlist = content.GetFromStream(stream);
            }
            return playlist;
        }

        public async Task<M3uPlaylist> OpenM3uPlaylist(StorageFile file)
        {
            M3uPlaylist playlist;
            using(var stream = await file.OpenStreamForReadAsync())
            {
                var content = new M3uContent();
                playlist = content.GetFromStream(stream);
            }
            return playlist;
        }

        public async Task<M3uPlaylist> OpenM3u8Playlist(StorageFile file)
        {
            M3uPlaylist playlist;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                var content = new M3u8Content();
                playlist = content.GetFromStream(stream);
            }
            return playlist;
        }

        public async Task<PlsPlaylist> OpenPlsPlaylist(StorageFile file)
        {
            PlsPlaylist playlist;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                var content = new PlsContent();
                playlist = content.GetFromStream(stream);
            }
            return playlist;
        }

        //error https://rink.hockeyapp.net/manage/apps/308671/app_versions/56/crash_reasons/151334432
        public async Task<WplPlaylist> OpenWplPlaylist(StorageFile file)
        {
            WplPlaylist playlist;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                var content = new WplContent();
                playlist = content.GetFromStream(stream);//error null reference
            }
            return playlist;
        }

        //error https://developer.microsoft.com/dashboard/analytics/reports/apphealth/details?productId=9NBLGGH67N4F&failureHash=c69459a7-1d77-1cdb-6e6e-71677cb5b9d8
        public async Task<ZplPlaylist> OpenZplPlaylist(StorageFile file)
        {
            ZplPlaylist playlist;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                var content = new ZplContent();
                playlist = content.GetFromStream(stream);
            }
            return playlist;
        }
    }
}
