using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Model
{
    public enum MusicItemTypes
    {
        album,
        artist,
        folder,
        genre,
        plainplaylist,
        smartplaylist,
        song
    }
    public abstract class MusicItem
    {
        public abstract string GetParameter();
        public const string separator = "!@#$%";
        public static MusicItemTypes ParseType(string param)
        {
            string[] s = param.Split(new string[] { separator },StringSplitOptions.None);
            return (MusicItemTypes)Enum.Parse(typeof(MusicItemTypes), s[0]);
        }
        public static string[] ParseParameter(string param)
        {
            return param.Split(new string[] { separator }, StringSplitOptions.None);
        }
    }
}
