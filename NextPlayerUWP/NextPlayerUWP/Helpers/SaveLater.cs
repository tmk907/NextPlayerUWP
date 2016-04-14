using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using NextPlayerUWPDataLayer.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextPlayerUWP.Common;

namespace NextPlayerUWP.Helpers
{
    public class SaveLater
    {
        private List<SongData> songs;
        private List<Tuple<int, int>> ratings;
        private List<Tuple<int, string>> cachedLyrics;

        private static SaveLater current = null;
        private SaveLater()
        {
            songs = new List<SongData>();
            ratings = new List<Tuple<int, int>>();
            cachedLyrics = new List<Tuple<int, string>>();
            object r = ApplicationSettingsHelper.ReadResetSettingsValue("savelaterratings");
            if (r != null)
            {
                string[] a = r.ToString().Split(new char[]{ '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (a.Length % 2 != 0)
                {
                    Logger.Save("SaveLater blad modulo");
                    Logger.SaveToFile();
                }
                else
                {
                    for (int i = 0; i < a.Length / 2; i++)
                    {
                        ratings.Add(new Tuple<int, int>(Int32.Parse(a[2 * i]), Int32.Parse(a[2 * i + 1])));
                    }
                }
            }
            object t = ApplicationSettingsHelper.ReadResetSettingsValue("savelatertags");
            if (t != null)
            {
                string[] a = t.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in a)
                {
                    songs.Add(DatabaseManager.Current.GetSongData(Int32.Parse(item)));
                }
            }
            object l = ApplicationSettingsHelper.ReadResetSettingsValue("savelaterlyrics");
            if (l != null)
            {
                string[] a = l.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var item in a)
                {
                    int id = Int32.Parse(item);
                    cachedLyrics.Add(new Tuple<int, string>(id, DatabaseManager.Current.GetLyrics(id)));
                }
            }
        }

        public async Task SaveAllNow()
        {
            await SaveLyricsNow();
            await SaveRatingsNow();
            await SaveTagsNow();
        }

        public static SaveLater Current
        {
            get
            {
                if (current == null)
                {
                    current = new SaveLater();
                }
                return current;
            }
        }

        public async Task SaveTagsNow()
        {
            List<SongData> l = new List<SongData>();
            if (songs.Count != 0)
            {
                var currSong = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                if (currSong == null) return;
                foreach (var item in songs)
                {
                    if (currSong.SongId != item.SongId)
                    {
                        await MediaImport.UpdateFileTags(item);
                    }
                    else
                    {
                        l.Add(item);
                    }
                }
                songs = new List<SongData>();
                ApplicationSettingsHelper.ReadResetSettingsValue("savelatertags");
                foreach (var item in l)
                {
                    SaveTagsLater(item);
                }
            }
        }

        public async Task SaveRatingsNow()
        {
            List<Tuple<int, int>> l = new List<Tuple<int, int>>();
            if (ratings.Count != 0)
            {
                var currSong = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                if (currSong == null) return;
                foreach (var item in ratings)
                {
                    if (currSong.SongId != item.Item1)
                    {
                        await MediaImport.UpdateRating(item.Item1, item.Item2);
                    }
                    else
                    {
                        l.Add(item);
                    }
                }
                ratings = new List<Tuple<int, int>>();
                ApplicationSettingsHelper.ReadResetSettingsValue("savelaterratings");
                foreach (var item in l)
                {
                    SaveRatingLater(item.Item1, item.Item2);
                }
            }
        }

        public async Task SaveLyricsNow()
        {
            List<Tuple<int, string>> l = new List<Tuple<int, string>>();
            if (cachedLyrics.Count != 0)
            {
                var currSong = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                if (currSong == null) return;
                foreach(var item in cachedLyrics)
                {
                    if (currSong.SongId != item.Item1)
                    {
                        await MediaImport.UpdateLyrics(item.Item1, item.Item2);
                    }
                    else
                    {
                        l.Add(item);
                    }
                }
                cachedLyrics = new List<Tuple<int, string>>();
                ApplicationSettingsHelper.ReadResetSettingsValue("savelaterlyrics");
                foreach(var item in l)
                {
                    SaveLyricsLater(item.Item1, item.Item2);
                }
            }
        }

        public void SaveTagsLater(SongData song)
        {
            object t = ApplicationSettingsHelper.ReadResetSettingsValue("savelatertags");
            if (t != null)
            {
                string[] a = t.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in a)
                {
                    songs.Add(DatabaseManager.Current.GetSongData(Int32.Parse(item)));
                }
            }



            bool duplicate = songs.Remove(song);
            songs.Add(song);
            if (!duplicate)
            {
                string val = (ApplicationSettingsHelper.ReadSettingsValue("savelatertags") ?? "").ToString() + song.SongId + "|";
                ApplicationSettingsHelper.SaveSettingsValue("savelatertags", val);
            }
        }

        public void SaveRatingLater(int songId, int rating)
        {
            bool duplicate = false;
            int oldRating = 0;
            foreach(var item in ratings)
            {
                if (item.Item1 == songId)
                {
                    duplicate = true;
                    oldRating = item.Item2;
                    break;
                }
            }
            string val = (ApplicationSettingsHelper.ReadSettingsValue("savelaterratings") ?? "").ToString();
            if (duplicate)
            {
                ratings.Remove(new Tuple<int, int>(songId, oldRating));
                val = val.Replace(songId + "|" + oldRating + "|", "");
            }
            ratings.Add(new Tuple<int, int>(songId, rating));
            ApplicationSettingsHelper.SaveSettingsValue("savelaterratings", val + songId + "|" + rating + "|");
        }

        public void SaveLyricsLater(int songId, string lyrics)
        {
            cachedLyrics.Add(new Tuple<int, string>(songId, lyrics));
            string val = (ApplicationSettingsHelper.ReadSettingsValue("savelaterlyrics") ?? "").ToString() + songId + "|";
            ApplicationSettingsHelper.SaveSettingsValue("savelaterlyrics", val);
        }
    }
}
