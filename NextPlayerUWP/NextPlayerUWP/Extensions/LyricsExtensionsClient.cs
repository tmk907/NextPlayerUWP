using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerUWP.Extensions
{
    public class LyricsExtensionsClient
    {
        private IExtensionsHelper extensionsHelper;

        public LyricsExtensionsClient(IExtensionsHelper extensionsHelper)
        {
            this.extensionsHelper = extensionsHelper;
        }

        public async Task<string> GetLyrics(string album, string artist, string title)
        {
            string lyrics = "";

            var extensions = await extensionsHelper.GetExtensionsInfo();
            if (extensions.Count == 0) return lyrics;

            ExtensionsService service = new ExtensionsService();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var data = JsonConvert.SerializeObject(new LyricsParameters()
            {
                Album = album,
                Artist = artist,
                Title = title,
            });
            parameters.Add(Commands.GetLyrics, data);

            foreach (var ext in extensions.OrderBy(e => e.Priority))
            {
                var response = await service.InvokeExtension(ext, parameters);
                if (response != null && response.ContainsKey(Responses.Lyrics))
                {
                    lyrics = response[Responses.Lyrics] as string;
                    break;
                }
            }

            return lyrics;
        }

        public async Task<string> GetLyrics(string album, string artist, string title, AppExtensionInfo extension)
        {
            string lyrics = "";
            ExtensionsService service = new ExtensionsService();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var data = JsonConvert.SerializeObject(new LyricsParameters()
            {
                Album = album,
                Artist = artist,
                Title = title,
            });
            parameters.Add(Commands.GetLyrics, data);

            var response = await service.InvokeExtension(extension, parameters);
            if (response != null && response.ContainsKey(Responses.Lyrics))
            {
                lyrics = response[Responses.Lyrics] as string;
            }

            return lyrics;
        }
    }
}
