using NextPlayerUWP.Common.Tiles;
using NextPlayerUWP.Extensions;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace NextPlayerUWP.Common
{
    public partial class PlaybackService
    {
        //MediaPlayer Player;
        //bool canPlay = false;
        public static event MediaPlayerTrackChangeHandler MediaPlayerTrackChanged;
        public void OnMediaPlayerTrackChanged(int index)
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerTrackChanged {0}", index);
            MediaPlayerTrackChanged?.Invoke(index);
            UpdateLiveTile(false);
            SendNotification();
        }

        private bool shuffle;
        private RepeatEnum repeat;

        private Stoper songPlayingStoper;

        private TimeSpan timePreviousOrBeggining = TimeSpan.FromSeconds(5);
        private LastFmCache lastFmCache;
        private PlaybackTimer RadioTimer;
        private PlaybackTimer MusicPlaybackTimer;


        #region Properties

        public int CurrentSongIndex
        {
            get
            {
                return ApplicationSettingsHelper.ReadSongIndex();
            }
            private set
            {
                ApplicationSettingsHelper.SaveSongIndex(value);
                OnMediaPlayerTrackChanged(value);
            }
        }

        public bool IsFirstIndex
        {
            get { return CurrentSongIndex == 0; }
        }

        public bool IsLastIndex
        {
            get { return CurrentSongIndex == NowPlayingPlaylistManager.Current.songs.Count - 1; }
        }

        public MediaPlaybackState PlayerState
        {
            get
            {
                return Player.PlaybackSession.PlaybackState;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return Player.PlaybackSession.NaturalDuration;
            }
        }

        public TimeSpan Position
        {
            get
            {
                return Player.PlaybackSession.Position;
            }
            set
            {
                var session = Player.PlaybackSession;
                if (session.CanSeek && value >= TimeSpan.Zero)
                {
                    if (value <= session.NaturalDuration)
                    {
                        session.Position = value;
                    }
                    else
                    {
                        session.Position = session.NaturalDuration;
                    }
                }
                else
                {

                }
            }
        }

        public int Volume
        {
            get
            {
                int vol = (int)(Player.Volume * 100.0);
                return vol;
            }
            set
            {
                if (value < 0 || value > 100)
                {
                    return;
                }
                double volume = value / 100.0;
                if (Player.Volume == volume) return;
                Player.Volume = volume;
                //ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.Volume, value);
            }
        }

        public double AudioBalance
        {
            get
            {
                return Player.AudioBalance;
            }
            set
            {
                if (value >= -1 && value <= 1)
                {
                    Player.AudioBalance = value;
                }
            }
        }

        public void ResetBalance()
        {
            Player.AudioBalance = 0;
        }

        public int PlaybackRatePercent
        {
            get
            {
                return (int)(Player.PlaybackSession.PlaybackRate * 100.0);
            }
            set
            {
                if (value >= 30 && value <= 400)
                {
                    double playbackRate = value / 100.0;
                    Player.PlaybackSession.PlaybackRate = playbackRate;
                }
            }
        }

        public void ResetPlaybackRate()
        {
            var session = Player.PlaybackSession;
            session.PlaybackRate = 1.0;
        }

        #endregion

        public async Task ChangeShuffle()
        {
            shuffle = Shuffle.CurrentState();
            System.Diagnostics.Debug.WriteLine("ChangeShuffle {0}", shuffle);
            if (!shuffle)
            {
                //await NowPlayingPlaylistManager.Current.UnShufflePlaylist();
                //mediaList.ShuffleEnabled = false;
                CurrentSongIndex = await NowPlayingPlaylistManager.Current.UnShufflePlaylist();
            }
            else
            {
                CurrentSongIndex = await NowPlayingPlaylistManager.Current.ShufflePlaylist(CurrentSongIndex);
                //mediaList.ShuffleEnabled = true;
            }
            await UpdateMediaListWithoutPausing();
        }

        public void ApplyRepeatState()
        {
            repeat = Repeat.CurrentState();
            if (repeat == RepeatEnum.RepeatOnce)
            {
                Player.IsLoopingEnabled = true;
                mediaList.AutoRepeatEnabled = false;
            }
            else if (repeat == RepeatEnum.RepeatPlaylist)
            {
                Player.IsLoopingEnabled = false;
                mediaList.AutoRepeatEnabled = true;
            }
            else
            {
                Player.IsLoopingEnabled = false;
                mediaList.AutoRepeatEnabled = false;
            }
            System.Diagnostics.Debug.WriteLine("ChangeRepeat {0}", repeat);
        }

        public void Next()
        {
            System.Diagnostics.Debug.WriteLine("Next");

            mediaList.MoveNext();
            OnMediaPlayerMediaOpened();
        }

        public void Previous()
        {
            System.Diagnostics.Debug.WriteLine("Previous");

            var session = Player.PlaybackSession;
            if (session.Position > timePreviousOrBeggining)
            {
                session.Position = TimeSpan.Zero;
            }
            else
            {
                mediaList.MovePrevious();
                OnMediaPlayerMediaOpened();
            }
        }

        public void TogglePlayPause()
        {
            switch (Player.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    Pause();
                    break;
                case MediaPlaybackState.Paused:
                    Play();
                    break;
                case MediaPlaybackState.Buffering:
                    if (Player.PlaybackSession.CanPause)
                    {
                        Pause();
                    }
                    break;
                case MediaPlaybackState.Opening:
                    break;
                default:
                    try
                    {
                        Play();
                    }
                    catch { }
                    break;
            }
        }

        public void Play()
        {
            Player.Play();
            songPlayingStoper.Start();
        }

        public void Pause()
        {
            Player.Pause();
            songPlayingStoper.Stop();
        }

        private const int fastForwardInterval = 5000;
        private const int rewindInterval = 5000;
        private const int fastForwardFasterInterval = 15000;
        private const int rewindFasterInterval = 15000;

        public void FastForward()
        {
            System.Diagnostics.Debug.WriteLine("FastForward");
            var session = Player.PlaybackSession;
            if (session.CanSeek)
            {
                var interval = TimeSpan.FromMilliseconds(fastForwardInterval);
                if (session.Position + interval + TimeSpan.FromMilliseconds(500) < session.NaturalDuration)
                {
                    session.Position += TimeSpan.FromMilliseconds(fastForwardInterval);
                }
                else
                {
                    Pause();
                    session.Position = TimeSpan.Zero;
                }
            }
        }

        public void Rewind()
        {
            System.Diagnostics.Debug.WriteLine("Rewind");
            var session = Player.PlaybackSession;
            if (session.CanSeek)
            {
                if (session.Position > TimeSpan.FromMilliseconds(rewindInterval))
                {
                    session.Position -= TimeSpan.FromMilliseconds(rewindInterval);
                }
                else
                {
                    Pause();
                    session.Position = TimeSpan.Zero;
                }
            }
        }

        #region Timer

        public void SetPlaybackStopTimer()
        {
            var t = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.TimerTime);
            long timerTicks = 0;
            if (t != null)
            {
                timerTicks = (long)t;
            }
            TimeSpan currentTime = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute) + TimeSpan.FromSeconds(DateTime.Now.Second);

            TimeSpan delay = TimeSpan.FromTicks(timerTicks - currentTime.Ticks);
            if (delay < TimeSpan.Zero)
            {
                delay = delay + TimeSpan.FromHours(24);
            }

            MusicPlaybackTimer.SetTimerWithAction(delay, PlaybackStopTimerCallback);
        }

        private void PlaybackStopTimerCallback()
        {
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerOn, false);
            Pause();
        }

        public void CancelPlaybackStopTimer()
        {
            MusicPlaybackTimer.TimerCancel();
        }

        #endregion

        private static async Task<MediaPlaybackItem> PreparePlaybackItem(SongItem song)
        {
            return await PlaybackItemBuilder.PreparePlaybackItem(song);
        }

        public static void Source_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Source_OpenOperationCompleted {0} {1}", sender.IsOpen, (args.Error?.ExtendedError.ToString()) ?? "");
            if (sender.IsOpen)
            {
                Instance.OnMediaPlayerMediaOpened();
            }
        }

        public static void RadioSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("RadioSource_OpenOperationCompleted {0}", sender.IsOpen);
            if (sender.IsOpen)
            {
                //Instance.RadioTimer.SetTimerWithTask(TimeSpan.FromMilliseconds(500), Instance.RefreshRadioTrackInfo);
            }
        }

        private int lastUpdatedTileSongId = -1;
        public void UpdateLiveTile(bool refresh)
        {
            if (NowPlayingPlaylistManager.Current.songs.Count == 0) return;
            int songIndex = CurrentSongIndex;
            var song = NowPlayingPlaylistManager.Current.songs[songIndex];
            if (!refresh && song.SongId == lastUpdatedTileSongId) return;
            lastUpdatedTileSongId = song.SongId;
            TileUpdateHelper tileHelper = new TileUpdateHelper();
            if (NowPlayingPlaylistManager.Current.songs.Count < 3)
            {
                tileHelper.UpdateAppTile(song.Title, song.Artist, song.AlbumArtUri.ToString());
            }
            else
            {
                var prevSong = NowPlayingPlaylistManager.Current.songs[(songIndex == 0) ? NowPlayingPlaylistManager.Current.songs.Count - 1 : songIndex - 1];
                var nextSong = NowPlayingPlaylistManager.Current.songs[(songIndex == NowPlayingPlaylistManager.Current.songs.Count - 1) ? 0 : songIndex + 1];
                List<string> titles = new List<string>() { prevSong.Title, song.Title, nextSong.Title };
                List<string> artists = new List<string>() { prevSong.Artist, song.Artist, nextSong.Artist };
                tileHelper.UpdateAppTile(titles, artists, song.AlbumArtUri.ToString());
            }
        }

        private NowPlayingBroadcasterExtension br = new NowPlayingBroadcasterExtension();
        private void SendNotification()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            br.SendNotification(song.Album, song.Artist, song.Title, PlaybackService.Instance.PlayerState);
        }

        private void UpdateStats(int songId, TimeSpan songDuration, TimeSpan songPlaybackDuration)
        {
            System.Diagnostics.Debug.WriteLine("UpdateStats {0}", songPlaybackDuration);
            if (songId == 1 || TimeSpan.Zero == songDuration)
            {
                System.Diagnostics.Debug.WriteLine("UpdateStats wrong song duration");
                return;
            }
            if (songPlaybackDuration.TotalSeconds >= songDuration.TotalSeconds * 0.5 || songPlaybackDuration.TotalSeconds >= 4 * 60)
            {
                UpdateStats2(songId, songDuration);
            }
        }

        private async Task UpdateStats2(int songId, TimeSpan songDuration)
        {
            var song = await DatabaseManager.Current.GetSongItemAsync(songId);
            if (song.SongId != -1 && song.SourceType != MusicSource.RadioJamendo && song.SourceType != MusicSource.Radio)
            {
                System.Diagnostics.Debug.WriteLine("UpdateStats2 {0} {1}", songId, songDuration);
                await UpdateSongStatistics(song.SongId);
                if (songDuration > TimeSpan.FromSeconds(30))
                {
                    await CacheTrackScrobble(song);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UpdateStats2 stats not updated");
            }
        }

        private async Task UpdateSongStatistics(int songId)
        {
            if (songId > 0)
            {
                await DatabaseManager.Current.UpdateSongStatistics(songId);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UpdateSongStatistics SongId < 0");
            }
        }

        private async Task CacheTrackScrobble(SongItem song)
        {
            int seconds = 0;
            try
            {
                DateTime start = DateTime.UtcNow - song.Duration;
                seconds = (int)start.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CacheTrackScrobble fails to calculate song playback start time {0}", ex);
                return;
            }
            TrackScrobble scrobble = new TrackScrobble()
            {
                Artist = song.Artist,
                Track = song.Title,
                Timestamp = seconds.ToString()
            };
            await lastFmCache.CacheTrackScrobble(scrobble).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("Scrobbled " + song.Artist + " " + song.Title);
        }
    }
}
