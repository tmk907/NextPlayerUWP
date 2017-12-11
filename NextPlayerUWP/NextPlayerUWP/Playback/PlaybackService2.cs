using NextPlayerUWP.Common;
using NextPlayerUWP.Common.Tiles;
using NextPlayerUWP.Extensions;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using NextPlayerUWPDataLayer.Services.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace NextPlayerUWP.Playback
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
            SendNotification();
        }

        private bool shuffle;
        private RepeatEnum repeat;

        private Stoper songPlayingStoper;

        private TimeSpan timePreviousOrBeggining = TimeSpan.FromSeconds(5);
        private ActionTimer RadioTimer;

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

        private NowPlayingBroadcasterExtension br = new NowPlayingBroadcasterExtension();
        private void SendNotification()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            br.SendNotification(song.Album, song.Artist, song.Title, PlaybackService.Instance.PlayerState);
        }


        ListeningHistoryRepository repo = new ListeningHistoryRepository();
        private async void UpdateStats(int songId, TimeSpan duration, TimeSpan playbackTime)
        {
            await repo.Add(new ListenedSong() { DatePlayed = DateTime.Now, PlaybackDuration = playbackTime, SongId = songId+10000 });
        }
    }
}
