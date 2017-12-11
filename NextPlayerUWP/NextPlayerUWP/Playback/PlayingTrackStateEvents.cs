using NextPlayerUWP.Common.Extensions;
using NextPlayerUWPDataLayer.Model;
using Windows.Media.Playback;

namespace NextPlayerUWP.Playback
{
    public class PlayingTrackStateEvents
    {
        public delegate void MediaPlayerTrackStartedHandler(SongItem song, MediaPlaybackState2 state);
        public delegate void MediaPlayerTrackPausedHandler();
        public delegate void MediaPlayerTrackResumedHandler();
        public delegate void MediaPlayerTrackCompletedHandler(SongItem song);

        public static event MediaPlayerTrackStartedHandler MediaPlayerTrackStarted;
        public static event MediaPlayerTrackPausedHandler MediaPlayerTrackPaused;
        public static event MediaPlayerTrackResumedHandler MediaPlayerTrackResumed;
        public static event MediaPlayerTrackCompletedHandler MediaPlayerTrackCompleted;

        private SongItem prevSong;
        private MediaPlaybackState prevState;

        public PlayingTrackStateEvents()
        {
            PlaybackService.MediaPlayerStateChanged += PlaybackService_MediaPlayerStateChanged;
            PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
            prevSong = new SongItem();
            prevState = MediaPlaybackState.None;
        }

        private void PlaybackService_MediaPlayerTrackChanged(int index)
        {
            if (prevSong.SongId != -1)
            {
                OnTrackCompleted(prevSong);
            }
            var song = NowPlayingPlaylistManager.Current.GetSongItem(index);
            OnTrackStarted(song, PlaybackService.Instance.PlayerState);
            prevSong = song;
        }

        private void PlaybackService_MediaPlayerStateChanged(MediaPlaybackState state)
        {
            switch (state)
            {
                case MediaPlaybackState.Paused:
                    OnTrackPaused();
                    break;
                case MediaPlaybackState.Playing:
                    if (prevState != MediaPlaybackState.Playing)
                    {
                        OnTrackResumed();
                    }
                    break;
                default:
                    break;
            }
            prevState = state;
        }

        private void OnTrackStarted(SongItem song, MediaPlaybackState state)
        {
            MediaPlayerTrackStarted?.Invoke(song, state.ToMyMediaPlaybackState());
        }

        private void OnTrackCompleted(SongItem song)
        {
            MediaPlayerTrackCompleted?.Invoke(song);
        }

        private void OnTrackPaused()
        {
            MediaPlayerTrackPaused?.Invoke();
        }

        private void OnTrackResumed()
        {
            MediaPlayerTrackResumed?.Invoke();
        }
    }
}
