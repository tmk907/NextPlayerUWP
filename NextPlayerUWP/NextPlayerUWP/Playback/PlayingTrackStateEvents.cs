using NextPlayerUWP.Common.Extensions;
using Windows.Media.Playback;

namespace NextPlayerUWP.Playback
{
    public class PlayingTrackStateEvents
    {
        public delegate void MediaPlayerTrackStartedHandler(int songId, MediaPlaybackState2 state);
        public delegate void MediaPlayerTrackPausedHandler();
        public delegate void MediaPlayerTrackResumedHandler();
        public delegate void MediaPlayerTrackCompletedHandler(int songId);

        public static event MediaPlayerTrackStartedHandler MediaPlayerTrackStarted;
        public static event MediaPlayerTrackPausedHandler MediaPlayerTrackPaused;
        public static event MediaPlayerTrackResumedHandler MediaPlayerTrackResumed;
        public static event MediaPlayerTrackCompletedHandler MediaPlayerTrackCompleted;

        private int prevSongId;
        private MediaPlaybackState prevState;

        public PlayingTrackStateEvents()
        {
            PlaybackService.MediaPlayerStateChanged += PlaybackService_MediaPlayerStateChanged;
            PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
            prevSongId = -1;
            prevState = MediaPlaybackState.None;
        }

        private void PlaybackService_MediaPlayerTrackChanged(int songId)
        {
            if (prevSongId != -1)
            {
                OnTrackCompleted(prevSongId);
            }
            OnTrackStarted(songId, PlaybackService.Instance.PlayerState);
            prevSongId = songId;
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

        private void OnTrackStarted(int songId, MediaPlaybackState state)
        {
            MediaPlayerTrackStarted?.Invoke(songId, state.ToMyMediaPlaybackState());
        }

        private void OnTrackCompleted(int songId)
        {
            MediaPlayerTrackCompleted?.Invoke(songId);
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
