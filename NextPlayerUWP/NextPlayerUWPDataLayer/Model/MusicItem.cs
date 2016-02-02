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
        song,
        unknown
    }
    public abstract class MusicItem
    {
        public abstract string GetParameter();
        public const string separator = "!@#$%";
        public static MusicItemTypes ParseType(string param)
        {
            string[] s = param.Split(new string[] { separator },StringSplitOptions.None);
            try
            {
                return (MusicItemTypes)Enum.Parse(typeof(MusicItemTypes), s[0]);
            }
            catch (Exception ex)
            {
                Diagnostics.Logger.Save("MusicItem ParseType " + Environment.NewLine + ex.Message);
                Diagnostics.Logger.SaveToFile();
                return MusicItemTypes.unknown;
            }
        }
        public static string[] ParseParameter(string param)
        {
            return param.Split(new string[] { separator }, StringSplitOptions.None);
        }
    }
}
