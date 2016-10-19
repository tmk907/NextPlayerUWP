namespace NextPlayerUWPDataLayer.Services
{
    public class AudioFormatsHelper
    {
        private bool isFFmpegSupported;
        public AudioFormatsHelper(bool isFFmpegSupported)
        {
            this.isFFmpegSupported = isFFmpegSupported;
        }

        public bool IsDefaultSupportedType(string type)
        {
            return (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                    type == ".wav" || type == ".aac" || type == ".asf" || type == ".flac" ||
                    type == ".adt" || type == ".adts" || type == ".amr" || type == ".mp4");
        }

        public bool IsFFmpegSupportedType(string type)
        {
            return (type == ".ogg" || type == ".ape" || type == ".wv" || type == ".opus" || type == ".ac3");
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
    }
}
