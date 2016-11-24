using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Services
{
    public class FileFormatsHelper
    {
        private bool isFFmpegSupported;
        public FileFormatsHelper(bool isFFmpegSupported)
        {
            this.isFFmpegSupported = isFFmpegSupported;
            defaultSupported = new List<string>()
                { ".mp3", ".m4a", ".wma", ".wav", ".aac", ".asf", ".flac", ".adt", ".adts", ".amr", ".mp4" };
            ffmpegSupported = new List<string>()
                { ".ogg", ".ape", ".wv", ".opus", ".ac3" };
            playlistFormats = new List<string>()
                { ".m3u", ".m3u8", ".pls", ".wpl", ".zpl" };
        }

        private readonly List<string> defaultSupported;
        private readonly List<string> ffmpegSupported;
        private readonly List<string> playlistFormats;

        public bool IsDefaultSupportedType(string type)
        {
            type = type.ToLower();
            return defaultSupported.Contains(type);
        }

        public bool IsFFmpegSupportedType(string type)
        {
            type = type.ToLower();
            return ffmpegSupported.Contains(type);
        }

        public bool IsFormatSupported(string type)
        {
            if (isFFmpegSupported)
            {
                return IsDefaultSupportedType(type) || IsFFmpegSupportedType(type);
            }
            else
            {
                return IsDefaultSupportedType(type);
            }
        }

        public bool IsPlaylistSupportedType(string type)
        {
            type = type.ToLower();
            return playlistFormats.Contains(type);
        }

        public List<string> SupportedAudioFormats()
        {
            var list = new List<string>(defaultSupported);
            if (isFFmpegSupported)
            {
                list.AddRange(ffmpegSupported);
            }
            return list;
        }

        public List<string> SupportedAudioAndPlaylistFormats()
        {
            var list = SupportedAudioFormats();
            list.AddRange(playlistFormats);
            return list;
        }
    }
}
