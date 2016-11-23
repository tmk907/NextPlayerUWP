using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using Windows.Graphics.Imaging;
using TagLib;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace NextPlayerUWP.Common
{
    public class MyFileAbstraction : TagLib.File.IFileAbstraction
    {
        public MyFileAbstraction(string name, Stream stream)
        {
            this.Name = name;
            this.ReadStream = stream;
            this.WriteStream = stream;
        }

        public MyFileAbstraction(string name, Stream rstream, Stream wstream)
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
                PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
            }
            else
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(songData.Path);
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (TagLib.File tagFile = TagLib.File.Create(new MyFileAbstraction(file.Name, fileStream.AsStream())))
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

        private static async void PlaybackService_MediaPlayerTrackChanged(int index)
        {
            PlaybackService.MediaPlayerTrackChanged -= PlaybackService_MediaPlayerTrackChanged;
            await SaveCached();
        }


        /// <summary>
        /// UI thread
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<WriteableBitmap> GetAlbumArt(StorageFile file)
        {
            WriteableBitmap bitmap = new WriteableBitmap(1, 1);
            try
            {
                Stream fileStream = await file.OpenStreamForReadAsync();
                TagLib.File tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                int picturesCount = tagFile.Tag.Pictures.Length;
                fileStream.Dispose();
                if (picturesCount > 0)
                {
                    IPicture pic = tagFile.Tag.Pictures[0];
                    using (MemoryStream stream = new MemoryStream(pic.Data.Data))
                    {
                        using (IRandomAccessStream istream = stream.AsRandomAccessStream())
                        {
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, istream);
                            bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                            istream.Seek(0);
                            await bitmap.SetSourceAsync(istream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageFile">png or jpg</param>
        /// <param name="tagfile"></param>
        /// <returns></returns>
        public async Task SaveAlbumArt(StorageFile imageFile, StorageFile tagfile)
        {
            if (imageFile == null) return;
            try
            {
                using (var fileStream = await tagfile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (TagLib.File tagFile = TagLib.File.Create(new MyFileAbstraction(tagfile.Name, fileStream.AsStream())))
                    {
                        using (var stream = await imageFile.OpenStreamForReadAsync())
                        {
                            Picture picture = new Picture();
                            picture.Type = PictureType.FrontCover;
                            picture.MimeType = (imageFile.Path.EndsWith(".png")) ? "image/png" : "image/jpeg";
                            picture.Description = "Cover";
                            picture.Data = TagLib.ByteVector.FromStream(stream);
                            tagFile.Tag.Pictures = new IPicture[] { picture };
                            tagFile.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task DeleteAlbumArt(StorageFile file)
        {
            try
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (TagLib.File tagFile = TagLib.File.Create(new MyFileAbstraction(file.Name, fileStream.AsStream())))
                    {
                        tagFile.Tag.Pictures = null;
                        tagFile.Tag.Pictures = new IPicture[0];
                        tagFile.Save();
                    }
                }
            }
            catch (Exception)
            {

            }
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
    }
}
