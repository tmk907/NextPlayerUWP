using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;

namespace NextPlayerUWP.Common
{
    public class OwnFileAbstraction : TagLib.File.IFileAbstraction
    {
        public OwnFileAbstraction(string name, Stream stream)
        {
            this.Name = name;
            this.ReadStream = stream;
            this.WriteStream = stream;
        }

        public OwnFileAbstraction(string name, Stream rstream, Stream wstream)
        {
            this.Name = name;
            this.ReadStream = rstream;
            this.WriteStream = wstream;
        }

        public void CloseStream(Stream stream)
        {
            stream.Flush();
        }

        public string Name
        {
            get;
            private set;
        }

        public Stream ReadStream
        {
            get;
            private set;
        }

        public Stream WriteStream
        {
            get;
            private set;
        }
    }

    public class TagsManager
    {
        public async Task SaveTags(SongData songData)
        {
            var current = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (current.SongId == songData.SongId)
            {
                SaveTagsLater(songData.SongId);
                PlaybackManager.MediaPlayerTrackChanged += PlaybackManager_MediaPlayerTrackChanged;
            }
            else
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(songData.Path);
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (TagLib.File tagFile = TagLib.File.Create(new OwnFileAbstraction(file.Name, fileStream.AsStream())))
                        {
                            tagFile.Tag.AlbumArtists = null;
                            tagFile.Tag.Composers = null;
                            tagFile.Tag.Performers = null;
                            tagFile.Tag.Genres = null;

                            tagFile.Tag.Album = songData.Tag.Album;
                            tagFile.Tag.AlbumArtists = new string[] { songData.Tag.AlbumArtist };
                            tagFile.Tag.Performers = songData.Tag.Artists.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            tagFile.Tag.Comment = songData.Tag.Comment;
                            tagFile.Tag.Composers = songData.Tag.Composers.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            tagFile.Tag.Conductor = songData.Tag.Conductor;
                            tagFile.Tag.Disc = (uint)songData.Tag.Disc;
                            tagFile.Tag.Genres = songData.Tag.Genres.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            tagFile.Tag.Title = songData.Tag.Title;
                            tagFile.Tag.Track = (uint)songData.Tag.Track;
                            tagFile.Tag.Year = (uint)songData.Tag.Year;
                            tagFile.Tag.Lyrics = songData.Tag.Lyrics;
                            tagFile.Save();
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    //Logger.Save("UpdateFileTags() " + Environment.NewLine + songData.Path + Environment.NewLine + ex.Message);
                    //Logger.SaveToFile();
                }
            }
        }

        private static async void PlaybackManager_MediaPlayerTrackChanged(int index)
        {
            PlaybackManager.MediaPlayerTrackChanged -= PlaybackManager_MediaPlayerTrackChanged;
            await SaveCached();
        }

        private void SaveTagsLater(int songId)
        {
            string t = (ApplicationSettingsHelper.ReadSettingsValue("savelatertags") ?? "").ToString();
            List<string> songIds = new List<string>();
            if (t != "")
            {
                string[] a = t.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in a)
                {
                    songIds.Add(item);
                }
            }
            bool duplicate = songIds.Remove(songId.ToString());
            if (!duplicate)
            {
                songIds.Add(songId.ToString());
                string val = t + songId + "|";
                ApplicationSettingsHelper.SaveSettingsValue("savelatertags", val);
            }
        }

        public static async Task SaveCached()
        {
            string t = (ApplicationSettingsHelper.ReadResetSettingsValue("savelatertags") ?? "").ToString();
            if (t != "")
            {
                TagsManager tm = new TagsManager();
                string[] a = t.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in a)
                {
                    SongData songData = await DatabaseManager.Current.GetSongDataAsync(Int32.Parse(item));
                    await tm.SaveTags(songData);
                }
            }
        }

        //using (Stream fileReadStream = await file.OpenStreamForReadAsync())
        //{
        //    using (Stream fileWriteStream = await file.OpenStreamForWriteAsync())
        //    {
        //        using (TagLib.File tagFile = TagLib.File.Create(new OwnFileAbstraction(file.Name, fileReadStream, fileWriteStream)))
        //        {
        //            tagFile.Tag.AlbumArtists = null;
        //            tagFile.Tag.Composers = null;
        //            tagFile.Tag.Performers = null;

        //            tagFile.Tag.Album = songData.Tag.Album;
        //            tagFile.Tag.AlbumArtists = new string[] { songData.Tag.AlbumArtist };
        //            tagFile.Tag.Performers = songData.Tag.Artists.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        //            tagFile.Tag.Comment = songData.Tag.Comment;
        //            tagFile.Tag.Composers = songData.Tag.Composers.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        //            tagFile.Tag.Conductor = songData.Tag.Conductor;
        //            tagFile.Tag.Disc = (uint)songData.Tag.Disc;
        //            tagFile.Tag.Genres = songData.Tag.Genres.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        //            tagFile.Tag.Title = songData.Tag.Title;
        //            tagFile.Tag.Track = (uint)songData.Tag.Track;
        //            tagFile.Tag.Year = (uint)songData.Tag.Year;
        //            tagFile.Save();
        //        }
        //    }
        //}
    }
}
