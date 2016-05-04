using System;

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
        nowplayinglist,
        unknown
    }
    public abstract class MusicItem
    {
        public int Index;
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
        public static string[] SplitParameter(string param)
        {
            return param.Split(new string[] { separator }, StringSplitOptions.None);
        }
    }
}
