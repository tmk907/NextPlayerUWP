﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerUWP.Extensions
{
    public class LyricsExtensionsClient
    {
        private IExtensionsHelper extensionsHelper;
        private ConcurrentDictionary<string, LyricsResponse> cache;

        public LyricsExtensionsClient(IExtensionsHelper extensionsHelper)
        {
            this.extensionsHelper = extensionsHelper;
            cache = new ConcurrentDictionary<string, LyricsResponse>();
        }

        public async Task<LyricsResponse> GetLyrics(string album, string artist, string title)
        {
            var extensions = await extensionsHelper.GetExtensionsInfo();
            LyricsResponse res = await GetLyrics(album, artist, title, extensions);
            return res;
        }

        public async Task<LyricsResponse> GetLyrics(string album, string artist, string title, AppExtensionInfo extension)
        {
            LyricsResponse res = await GetLyrics(album, artist, title, new List<AppExtensionInfo>() { extension });
            return res;
        }

        private async Task<LyricsResponse> GetLyrics(string album, string artist, string title, IEnumerable<AppExtensionInfo> extensions)
        {
            string key = ParamsToKey(album, artist, title);
            LyricsResponse res;
            if (cache.TryGetValue(key, out res))
            {
                return res;
            }
            else
            {
                res = new LyricsResponse() { Lyrics = "", Url = "" };
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var data = JsonConvert.SerializeObject(new LyricsRequest()
            {
                Album = album,
                Artist = artist,
                Title = title,
            });
            parameters.Add(Commands.GetLyrics, data);

            ExtensionsService service = new ExtensionsService();

            foreach (var ext in extensions.Where(e => e.Enabled).OrderBy(e => e.Priority))
            {
                var response = await service.InvokeExtension(ext, parameters);
                if (response != null && response.ContainsKey(Responses.Result))
                {
                    res = JsonConvert.DeserializeObject<LyricsResponse>(response[Responses.Result] as string);
                    if (!String.IsNullOrEmpty(res.Lyrics) || !String.IsNullOrEmpty(res.Url))
                    {
                        cache.TryAdd(key, res);
                        break;
                    }
                }
            }

            return res;
        }

        private string ParamsToKey(string p1, string p2, string p3)
        {
            return p1 + "/" + p2 + "/" + p3;
        }
    }
}