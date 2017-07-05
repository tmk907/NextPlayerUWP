using System;
using System.Threading;
using System.Threading.Tasks;
using UWPMusicPlayerExtensions.Client;
using UWPMusicPlayerExtensions.Enums;
using UWPMusicPlayerExtensions.Messages;
using Windows.Media.Playback;

namespace NextPlayerUWP.Extensions
{
    public class NowPlayingBroadcasterExtension
    {
        ExtensionClientHelper helper;
        NowPlayingNotificationBroadcaster client;
        CancellationTokenSource cts;

        string prevData = "";

        public NowPlayingBroadcasterExtension()
        {
            cts = new CancellationTokenSource();
            helper = new ExtensionClientHelper(MusicPlayerExtensionTypes.NowPlayingNotification);
            client = new NowPlayingNotificationBroadcaster(helper);
        }

        public async Task SendNotification(string album, string artist, string title, MediaPlaybackState state)
        {
            string data = album + artist + title + state.ToString();
            if (data == prevData) return;
            cts.Cancel();
            cts = new CancellationTokenSource();
            try
            {
                prevData = data;
                await client.SendRequestAsync(new NowPlayingNotification()
                {
                    Album = album,
                    Artist = artist,
                    AlbumArt = "",
                    Status = ToPlaybackStatus(state),
                    Title = title,
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private PlaybackStatus ToPlaybackStatus(MediaPlaybackState state)
        {
            switch (state)
            {
                case MediaPlaybackState.Buffering:
                    return PlaybackStatus.Changing;
                case MediaPlaybackState.None:
                    return PlaybackStatus.Closed;
                case MediaPlaybackState.Opening:
                    return PlaybackStatus.Changing;
                case MediaPlaybackState.Paused:
                    return PlaybackStatus.Paused;
                case MediaPlaybackState.Playing:
                    return PlaybackStatus.Playing;
                default:
                    return PlaybackStatus.Closed;
            }
        }
    }
}
