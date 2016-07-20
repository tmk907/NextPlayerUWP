using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using NextPlayerUWPDataLayer.Constants;
using System.Diagnostics;
using Windows.Storage.Search;

namespace NextPlayerUWPDataLayer.Services
{
    public class ImagesManager
    {
        public static async Task<string> SaveAlbumCover(string album, string albumArtist,string tileId)
        {
            var songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(album, albumArtist);
            string path = songs.FirstOrDefault().Path;
            string name = await SaveSongCover(path, tileId);
            return name;
        }

        /// <summary>
        /// Read cover from file, and if exists save it in app folder, return saved image path or default app tile path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<string> SaveSongCover(string path, string fileName)
        {
            string imagePath = "";

            int a = 0;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                Stream fileStream = await file.OpenStreamForReadAsync();
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                a = tagFile.Tag.Pictures.Length;

                if (a > 0)
                {
                    IPicture pic = tagFile.Tag.Pictures[0];
                    MemoryStream stream = new MemoryStream(pic.Data.Data);

                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    StorageFile storageFile = await localFolder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.ReplaceExisting);
                    imagePath = "ms-appdata:///local/" + fileName + ".jpg";
                    stream.Seek(0, SeekOrigin.Begin);
                    using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
                    {
                        stream.CopyTo(outputStream);
                    }
                }
                else
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                    try
                    {
                        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                        if (files.Count > 0)
                        {
                            foreach (var x in files)
                            {
                                if (x.Path.EndsWith("jpg"))
                                {
                                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                                    StorageFile storageFile = await localFolder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.ReplaceExisting);
                                    imagePath = "ms-appdata:///local/" + fileName + ".jpg";
                                    using (Stream stream = await x.OpenStreamForReadAsync())
                                    {
                                        using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
                                        {
                                            stream.CopyTo(outputStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            catch (Exception e)
            {

            }
            if (imagePath == "")
            {
                imagePath = "ms-appx:///Assets/AppImages/Logo/LogoTr.png";
            }
            return imagePath;
        }

        /// <summary>
        /// Return cover saved in file tags or .jpg from folder.
        /// If cover doesn't exist width and height are 0px.
        /// UI Thread
        /// </summary>
        public static async Task<BitmapImage> GetOriginalCover(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            int a = 0;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                Stream fileStream = await file.OpenStreamForReadAsync();
                TagLib.File tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                a = tagFile.Tag.Pictures.Length;
                fileStream.Dispose();
                if (a > 0)
                {
                    IPicture pic = tagFile.Tag.Pictures[0];
                    using (MemoryStream stream = new MemoryStream(pic.Data.Data))
                    {
                        using (IRandomAccessStream istream = stream.AsRandomAccessStream())
                        {
                            await bitmap.SetSourceAsync(istream);
                        }
                    }
                }
                else
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                    try
                    {
                        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                        if (files.Count > 0)
                        {
                            foreach (var x in files)
                            {
                                if (x.Path.EndsWith("jpg"))
                                {
                                    using (IRandomAccessStream stream = await x.OpenAsync(FileAccessMode.Read))
                                    {
                                        await bitmap.SetSourceAsync(stream);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            catch (Exception e)
            {

            }
            return bitmap;
        }

        /// <summary>
        /// Return default cover.
        /// UI Thread
        /// </summary>
        public static async Task<BitmapImage> GetDefaultCover(bool small = true)
        {
            BitmapImage bitmap = new BitmapImage();
            Uri uri;
            if (small)
            {
                uri = new Uri(AppConstants.AlbumCoverSmall);
            }
            else
            {
                uri = new Uri(AppConstants.AlbumCover);
            }
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                await bitmap.SetSourceAsync(stream);
            }
            return bitmap;
        }

        /// <summary>
        /// Return cover saved in file tags or .jpg from folder or default cover.
        /// UI Thread
        /// </summary>
        public static async Task<BitmapImage> GetCover(string path, bool isSmall = true)
        {
            BitmapImage bitmap = new BitmapImage();

            if (string.IsNullOrEmpty(path))
            {
                bitmap = await GetDefaultCover(isSmall);
            }
            else
            {
                bitmap = await GetOriginalCover(path);
            }

            if (bitmap.PixelHeight == 0)
            {
                bitmap = await GetDefaultCover(isSmall);
            }

            return bitmap;
        }

        /// <summary>
        /// UI Thread
        /// fileName without extension
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folderName"></param>
        /// <param name="image"></param>
        /// <returns>file path "ms-appdata:///local/..."</returns>
        public static async Task<string> SaveCover(string fileName, string folderName, WriteableBitmap image)
        {
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            StorageFile file = await folder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);

            try
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    var pixelStream = image.PixelBuffer.AsStream();
                    byte[] pixels = new byte[image.PixelBuffer.Length];

                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)image.PixelWidth, (uint)image.PixelHeight, 96, 96, pixels);

                    await encoder.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                //image exists and is opened in album view
            }
            return "ms-appdata:///local/" + folderName + "/" + fileName + ".jpg";
        }

        /// <summary>
        /// UI Thread
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static async Task<BitmapImage> GetCachedCover(string path)
        {
            BitmapImage image = new BitmapImage();
            if (path == "") return image;//!
            Uri uri = new Uri(path);

            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                await image.SetSourceAsync(stream);
            }

            return image;
        }

        /// <summary>
        /// UI Thread
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        public static async Task<string> GetAlbumCoverPath(AlbumItem album)
        {
            string imagePath = AppConstants.AlbumCover;
            if (!album.IsImageSet)
            {
                var songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(album.AlbumParam, album.AlbumArtist);
                var song = songs.FirstOrDefault(s => s.IsAlbumArtSet && s.CoverPath != AppConstants.AlbumCover);
                if (song == null)
                {
                    foreach(var s in songs.Where(x => !x.IsAlbumArtSet))
                    {
                        await PrepareSongAlbumArt(s);
                        if (s.CoverPath != AppConstants.AlbumCover)
                        {
                            imagePath = s.CoverPath;
                            break;
                        }
                    }
                }
                else
                {
                    imagePath = song.CoverPath;
                }
            }
            else
            {
                imagePath = album.ImagePath;
            }

            return imagePath;
        }

        public static long i1 = 0;
        public static long i2 = 0;

        public static async Task PrepareSongAlbumArt(SongItem song)
        {
            //Stopwatch s = new Stopwatch();
            //s.Start();
            //var cover = await GetAlbumArtBitmap(song.Path);
            var cover = await GetAlbumArtBitmap2(song.Path);

            //s.Stop();
            //i1 += s.ElapsedMilliseconds;
            //s.Restart();

            if (cover == null || cover.PixelHeight == 1)
            {
                song.CoverPath = AppConstants.AlbumCover;
            }
            else
            {
                string p = await SaveCover(song.SongId.ToString(), "Songs", cover);
                song.CoverPath = p;
            }
            //s.Stop();
            ////Debug.WriteLine("Prepare 1 {0} 2 {1}", b, s.ElapsedMilliseconds);
            //i2 += s.ElapsedMilliseconds;
            song.IsAlbumArtSet = true;
        }

        /// <summary>
        /// UI Thread
        /// Search for album art in tags, folder, thumbnail
        /// If no album art is find return null
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> GetAlbumArtBitmap2(string path)
        {
            try 
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                StorageFile songFile = await StorageFile.GetFileFromPathAsync(path);
                // <5ms
                bool set = false;
                var thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
                if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
                {
                    if (thumb.OriginalWidth > 200)
                    {
                        using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                        {
                            bitmap = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                            istream.Seek(0);
                            bitmap.SetSource(istream);
                        }
                        set = true;
                        //return bitmap;
                    }
                }
                if (!set)
                {
                    try
                    {
                        Stream fileStream = await songFile.OpenStreamForReadAsync();
                        TagLib.File tagFile = TagLib.File.Create(new StreamFileAbstraction(songFile.Name, fileStream, fileStream));
                        int picturesCount = tagFile.Tag.Pictures.Length;
                        fileStream.Dispose();
                        if (picturesCount > 0)
                        {
                            IPicture pic = tagFile.Tag.Pictures[0];
                            using (MemoryStream stream = new MemoryStream(pic.Data.Data))
                            {
                                using (IRandomAccessStream istream = stream.AsRandomAccessStream())
                                {
                                    bitmap = await WriteableBitmapExtensions.FromStream(new WriteableBitmap(1, 1), istream);
                                }
                            }
                            set = true;
                            //return bitmap;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!set)
                    {
                        try
                        {
                            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                            // not  exist
                            // 15ms 30 ms
                            QueryOptions q = new QueryOptions(CommonFileQuery.DefaultQuery, new List<string>() { ".jpg", ".png", ".jpeg" });
                            var res = folder.CreateFileQueryWithOptions(q);
                            var files2 = await res.GetFilesAsync();
                            if (files2.Count > 0)
                            {
                                using (IRandomAccessStream istream = await files2.FirstOrDefault().OpenAsync(FileAccessMode.Read))
                                {
                                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                                    bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                                    istream.Seek(0);
                                    await bitmap.SetSourceAsync(istream);
                                }
                                //return bitmap;
                                set = true;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                if (!set && thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
                {
                    using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                    {
                        bitmap = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                        istream.Seek(0);
                        bitmap.SetSource(istream);
                    }
                    set = true;
                }
                return bitmap;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// UI Thread
        /// Search for album art in tags, folder, thumbnail
        /// If no album art is find return null
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> GetAlbumArtBitmap(string path)
        {
            try
            {
                int picturesCount = 0;
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                
                try
                {
                    //Stopwatch st = new Stopwatch();
                    //st.Start();
                    StorageFile songFile = await StorageFile.GetFileFromPathAsync(path);
                    Stream fileStream = await songFile.OpenStreamForReadAsync();
                    TagLib.File tagFile = TagLib.File.Create(new StreamFileAbstraction(songFile.Name, fileStream, fileStream));
                    picturesCount = tagFile.Tag.Pictures.Length;
                    fileStream.Dispose();
                    if (picturesCount > 0)
                    {
                        IPicture pic = tagFile.Tag.Pictures[0];
                        using (MemoryStream stream = new MemoryStream(pic.Data.Data))
                        {
                            using (IRandomAccessStream istream = stream.AsRandomAccessStream())
                            {
                                bitmap = await WriteableBitmapExtensions.FromStream(new WriteableBitmap(1, 1), istream);
                            }
                        }
                        //st.Stop();
                        //Debug.WriteLine("Tag {0}ms", st.ElapsedMilliseconds);
                    }                  
                    else
                    {
                        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                        try
                        {
                            // not  exist
                            // 15ms 30 ms
                            QueryOptions q = new QueryOptions(CommonFileQuery.DefaultQuery, new List<string>() { ".jpg", ".png", ".jpeg" });
                            var res = folder.CreateFileQueryWithOptions(q);
                            var files2 = await res.GetFilesAsync();
                            if (files2.Count > 0)
                            {
                                //Stopwatch s = new Stopwatch();
                                //s.Start();
                                //using (IRandomAccessStream istream = await files2.FirstOrDefault().OpenAsync(FileAccessMode.Read))
                                //{
                                //    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                                //    bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                                //    istream.Seek(0);
                                //    bitmap.SetSource(istream);
                                //}
                                //s.Stop();
                                //var b = s.ElapsedMilliseconds;
                                //s.Restart();
                                using (IRandomAccessStream istream = await files2.FirstOrDefault().OpenAsync(FileAccessMode.Read))
                                {
                                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                                    bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                                    istream.Seek(0);
                                    await bitmap.SetSourceAsync(istream);
                                }
                                //s.Stop();
                                //Debug.WriteLine("Folder 1 {0}ms 2 {1}ms", b, s.ElapsedMilliseconds);
                            }
                            //25ms 50ms
                            //IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                            //var file = files.FirstOrDefault(f => f.Path.EndsWith("jpg") || f.Path.EndsWith("png") || f.Path.EndsWith("jpeg"));
                            //if (file != null)
                            //{
                            //    using (IRandomAccessStream istream = await file.OpenAsync(FileAccessMode.Read)) //5-20ms
                            //    {
                            //        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                            //        bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                            //        istream.Seek(0);
                            //        bitmap.SetSource(istream);
                            //    }
                            //}
                        }
                        catch (Exception e)
                        {

                        }
                        //st.Stop();
                        //Debug.WriteLine("Folder {0}ms", st.ElapsedMilliseconds);
                    }
                    if (bitmap.PixelHeight == 1)
                    {
                        //Stopwatch s = new Stopwatch();
                        //s.Start();
                        try // <5ms
                        {
                            var thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
                            if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
                            {
                                using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                                {
                                    bitmap = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                                    istream.Seek(0);
                                    bitmap.SetSource(istream);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        //s.Stop();
                        //Debug.WriteLine("Thumb {0}ms", s.ElapsedMilliseconds);
                    }
                    //else
                    //{
                    //    var thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
                    //    if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
                    //    {
                    //        Debug.WriteLine("Sizes Tag: {0}x{1} Thumb: {2}x{3}", bitmap.PixelHeight, bitmap.PixelWidth, thumb.OriginalHeight, thumb.OriginalWidth);
                    //    }
                    //    else
                    //    {
                    //        Debug.WriteLine("no thumbnail");
                    //    }
                    //}

                }
                catch (Exception e)
                {

                }
                return bitmap;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
