using System.Threading;
using System.Threading.Tasks;
using UWPMusicPlayerExtensions.Client;
using UWPMusicPlayerExtensions.Enums;
using UWPMusicPlayerExtensions.Messages;

namespace NextPlayerUWP.Extensions
{
    public class MyLyricsExtensionsClient
    {
        private LyricsExtensionsClient client;
        private ExtensionClientHelper helper;
        private LyricsExtensions lyricsExtensions;

        public MyLyricsExtensionsClient()
        {
            helper = new ExtensionClientHelper(ExtensionTypes.Lyrics);
            client = new LyricsExtensionsClient(helper);
            lyricsExtensions = new LyricsExtensions(helper);
        }

        public LyricsExtensions GetHelper() { return lyricsExtensions; }

        public async Task<LyricsResponse> GetLyrics(string album, string artist, string title, CancellationToken token)
        {
            var extensions = await lyricsExtensions.GetExtensionsInfo();

            LyricsRequest request = new LyricsRequest()
            {
                Album = album,
                Artist = artist,
                Title = title,
                PreferSynchronized = false,
            };

            return await client.SendRequestAsync(request, extensions, token);
        }
    }
}
