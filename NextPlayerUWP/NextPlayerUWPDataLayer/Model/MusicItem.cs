using System;

namespace NextPlayerUWPDataLayer.Model
{
    public enum MusicItemTypes
    {
        album,
        artist,
        folder,
        genre,
        nowplayinglist,
        onedrivefolder,
        googledrivefolder,
        pcloudfolder,
        dropboxfolder,
        plainplaylist,
        radio,
        smartplaylist,
        song,
        listofsongs,
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
                Diagnostics.Logger2.Current.WriteMessage("MusicItem ParseType " + Environment.NewLine + ex.Message);
                return MusicItemTypes.unknown;
            }
        }
        public static string[] SplitParameter(string param)
        {
            return param.Split(new string[] { separator }, StringSplitOptions.None);
        }
    }

    public class ListOfSongs : MusicItem
    {
        public override string GetParameter()
        {
            return MusicItemTypes.listofsongs + separator + "a";
        }
    }

}
