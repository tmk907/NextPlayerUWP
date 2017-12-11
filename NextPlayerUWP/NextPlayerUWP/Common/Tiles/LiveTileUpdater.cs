using NextPlayerUWP.Playback;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Common.Tiles
{
    public class LiveTileUpdater
    {
        private int lastUpdatedTileSongId = -1;

        public LiveTileUpdater()
        {
            PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
        }

        private async void PlaybackService_MediaPlayerTrackChanged(int index)
        {
            await UpdateLiveTile(false);
        }

        public async Task UpdateLiveTile(bool forceRefresh)
        {
            if (NowPlayingPlaylistManager.Current.songs.Count == 0) return;
            int songIndex = PlaybackService.Instance.CurrentSongIndex;
            var song = NowPlayingPlaylistManager.Current.songs[songIndex];
            if (song.SongId == lastUpdatedTileSongId && !forceRefresh) return;
            lastUpdatedTileSongId = song.SongId;
            TileUpdateHelper tileHelper = new TileUpdateHelper();
            if (NowPlayingPlaylistManager.Current.songs.Count < 3)
            {
                await tileHelper.UpdateAppTile(song.Title, song.Artist, song.AlbumArtUri.ToString());
            }
            else
            {
                var prevSong = NowPlayingPlaylistManager.Current.songs[(songIndex == 0) ? NowPlayingPlaylistManager.Current.songs.Count - 1 : songIndex - 1];
                var nextSong = NowPlayingPlaylistManager.Current.songs[(songIndex == NowPlayingPlaylistManager.Current.songs.Count - 1) ? 0 : songIndex + 1];
                List<string> titles = new List<string>() { prevSong.Title, song.Title, nextSong.Title };
                List<string> artists = new List<string>() { prevSong.Artist, song.Artist, nextSong.Artist };
                await tileHelper.UpdateAppTile(titles, artists, song.AlbumArtUri.ToString());
            }
        }
    }
}
