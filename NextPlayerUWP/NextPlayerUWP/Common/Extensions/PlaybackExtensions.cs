using NextPlayerUWP.Playback;
using Windows.Media.Playback;

namespace NextPlayerUWP.Common.Extensions
{
    public static class PlaybackExtensions
    {
        public static MediaPlaybackState2 ToMyMediaPlaybackState(this MediaPlaybackState state)
        {
            switch (state)
            {
                case MediaPlaybackState.Buffering:
                    return MediaPlaybackState2.Buffering;
                case MediaPlaybackState.None:
                    return MediaPlaybackState2.None;
                case MediaPlaybackState.Opening:
                    return MediaPlaybackState2.Opening;
                case MediaPlaybackState.Paused:
                    return MediaPlaybackState2.Paused;
                case MediaPlaybackState.Playing:
                    return MediaPlaybackState2.Playing;
                default:
                    return MediaPlaybackState2.None;
            }
        }
    }
}
