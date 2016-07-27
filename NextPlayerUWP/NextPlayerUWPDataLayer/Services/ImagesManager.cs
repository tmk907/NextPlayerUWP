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
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;

namespace NextPlayerUWPDataLayer.Services
{
    public class ImagesManager
    {
        public static async Task<string> SaveTileAlbumCover(string albumName, string albumArtist,string tileId)
        {
            var album = await DatabaseManager.Current.GetAlbumItemAsync(albumName, albumArtist);
            if (album.IsImageSet) return album.ImagePath;

            var songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(albumName, albumArtist);
            var song = songs.FirstOrDefault(s => s.IsAlbumArtSet);
            if (song != null) return song.CoverPath;

            song = songs.FirstOrDefault();
            var bmp = await GetAlbumArtBitmap(song.Path, true);
            string name = "ms-appx:///Assets/AppImages/Logo/LogoTr.png";

            if (bmp != null)
            {
                name = await SaveCover(tileId, "TileImages", bmp);
            }
            
            //string name = await SaveSongCover(path, tileId);
            return name;
        }

        /// <summary>
        /// UI Thread
        /// fileName without extension
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folderName"></param>
        /// <param name="image"></param>
        /// <returns>file path "ms-appdata:///local/..."</returns>
        public static async Task<string> SaveCover(string fileName, string folderName, WriteableBitmap image, int resize = 0)
        {
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            StorageFile file = await folder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);

            try
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                    int width = image.PixelWidth;
                    int height = image.PixelHeight;

                    if (resize != 0)
                    {
                        if (width > resize)
                        {
                            if (width == height)
                            {
                                height = resize;
                            }
                            else
                            {
                                height = height * resize / width;
                            }
                            width = resize;

                            image = image.Resize(width, height, WriteableBitmapExtensions.Interpolation.Bilinear);
                        }
                    }

                    using (var pixelStream = image.PixelBuffer.AsStream())
                    {
                        byte[] pixels = new byte[image.PixelBuffer.Length];

                        await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, pixels);

                        await encoder.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                //image exists and is opened in album view
            }
            return "ms-appdata:///local/" + folderName + "/" + fileName + ".jpg";
        }

        public static string GetHash(WriteableBitmap bmp)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var hashed = alg.HashData(bmp.PixelBuffer);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }

        public static async Task SaveAlbumArtFromSong(SongItem song)
        {
            var cover = await GetAlbumArtBitmap(song.Path);

            if (cover == null)
            {
                song.CoverPath = AppConstants.AlbumCover;
            }
            else
            {
                string p = await SaveCover(song.SongId.ToString(), "Songs", cover);
                song.CoverPath = p;
            }

            song.IsAlbumArtSet = true;
        }

        public static async Task SaveAlbumArtFromSong(SongData song)
        {
            var cover = await GetAlbumArtBitmap(song.Path);

            if (cover == null)
            {
                song.AlbumArtPath = AppConstants.AlbumCover;
            }
            else
            {
                string p = await SaveCover(song.SongId.ToString(), "Songs", cover);
                song.AlbumArtPath = p;
            }
        }

        /// <summary>
        /// UI Thread
        /// Search for album art in tags, folder, thumbnail
        /// If no album art is find return null
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> GetAlbumArtBitmap(string path, bool searchInThumbnail = false)
        {
            try 
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                StorageFile songFile = await StorageFile.GetFileFromPathAsync(path);
                bool set = false;
                // <5ms
                var thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
                if (searchInThumbnail)
                {
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
                        }
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
                            List<string> acceptedFileNames = new List<string>() { "cover", "album", "front", "albumart", "album art", "album-art" };
                            var files3 = files2.Where(f => acceptedFileNames.Contains(f.DisplayName.ToLower()));
                            if (files3.Count() > 0)
                            {
                                using (IRandomAccessStream istream = await files3.FirstOrDefault().OpenAsync(FileAccessMode.Read))
                                {
                                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                                    bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                                    istream.Seek(0);
                                    await bitmap.SetSourceAsync(istream);
                                }
                                set = true;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                if (searchInThumbnail && !set && thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image && thumb.OriginalHeight > 100)
                {
                    using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                    {
                        bitmap = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                        istream.Seek(0);
                        bitmap.SetSource(istream);
                    }
                    set = true;
                }
                if (set) return bitmap;
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        #region old

        ///// <summary>
        ///// Read cover from file, and if exists save it in app folder, return saved image path or default app tile path
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public static async Task<string> SaveSongCover(string path, string fileName)
        //{
        //    string imagePath = "";

        //    int a = 0;
        //    try
        //    {
        //        StorageFile file = await StorageFile.GetFileFromPathAsync(path);
        //        Stream fileStream = await file.OpenStreamForReadAsync();
        //        var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
        //        a = tagFile.Tag.Pictures.Length;

        //        if (a > 0)
        //        {
        //            IPicture pic = tagFile.Tag.Pictures[0];
        //            MemoryStream stream = new MemoryStream(pic.Data.Data);

        //            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        //            StorageFile storageFile = await localFolder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.ReplaceExisting);
        //            imagePath = "ms-appdata:///local/" + fileName + ".jpg";
        //            stream.Seek(0, SeekOrigin.Begin);
        //            using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
        //            {
        //                stream.CopyTo(outputStream);
        //            }
        //        }
        //        else
        //        {
        //            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
        //            try
        //            {
        //                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
        //                if (files.Count > 0)
        //                {
        //                    foreach (var x in files)
        //                    {
        //                        if (x.Path.EndsWith("jpg"))
        //                        {
        //                            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        //                            StorageFile storageFile = await localFolder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.ReplaceExisting);
        //                            imagePath = "ms-appdata:///local/" + fileName + ".jpg";
        //                            using (Stream stream = await x.OpenStreamForReadAsync())
        //                            {
        //                                using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
        //                                {
        //                                    stream.CopyTo(outputStream);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    if (imagePath == "")
        //    {
        //        imagePath = "ms-appx:///Assets/AppImages/Logo/LogoTr.png";
        //    }
        //    return imagePath;
        //}

        ///// <summary>
        ///// Return cover saved in file tags or .jpg from folder.
        ///// If cover doesn't exist width and height are 0px.
        ///// UI Thread
        ///// </summary>
        //public static async Task<BitmapImage> GetOriginalCover(string path)
        //{
        //    BitmapImage bitmap = new BitmapImage();
        //    int a = 0;
        //    try
        //    {
        //        StorageFile file = await StorageFile.GetFileFromPathAsync(path);
        //        Stream fileStream = await file.OpenStreamForReadAsync();
        //        TagLib.File tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
        //        a = tagFile.Tag.Pictures.Length;
        //        fileStream.Dispose();
        //        if (a > 0)
        //        {
        //            IPicture pic = tagFile.Tag.Pictures[0];
        //            using (MemoryStream stream = new MemoryStream(pic.Data.Data))
        //            {
        //                using (IRandomAccessStream istream = stream.AsRandomAccessStream())
        //                {
        //                    await bitmap.SetSourceAsync(istream);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
        //            try
        //            {
        //                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
        //                if (files.Count > 0)
        //                {
        //                    foreach (var x in files)
        //                    {
        //                        if (x.Path.EndsWith("jpg"))
        //                        {
        //                            using (IRandomAccessStream stream = await x.OpenAsync(FileAccessMode.Read))
        //                            {
        //                                await bitmap.SetSourceAsync(stream);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    return bitmap;
        //}

        ///// <summary>
        ///// Return default cover.
        ///// UI Thread
        ///// </summary>
        //public static async Task<BitmapImage> GetDefaultCover(bool small = true)
        //{
        //    BitmapImage bitmap = new BitmapImage();
        //    Uri uri;
        //    if (small)
        //    {
        //        uri = new Uri(AppConstants.AlbumCoverSmall);
        //    }
        //    else
        //    {
        //        uri = new Uri(AppConstants.AlbumCover);
        //    }
        //    var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
        //    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
        //    {
        //        await bitmap.SetSourceAsync(stream);
        //    }
        //    return bitmap;
        //}

        ///// <summary>
        ///// Return cover saved in file tags or .jpg from folder or default cover.
        ///// UI Thread
        ///// </summary>
        //public static async Task<BitmapImage> GetCover(string path, bool isSmall = true)
        //{
        //    BitmapImage bitmap = new BitmapImage();

        //    if (string.IsNullOrEmpty(path))
        //    {
        //        bitmap = await GetDefaultCover(isSmall);
        //    }
        //    else
        //    {
        //        bitmap = await GetOriginalCover(path);
        //    }

        //    if (bitmap.PixelHeight == 0)
        //    {
        //        bitmap = await GetDefaultCover(isSmall);
        //    }

        //    return bitmap;
        //}

        ///// <summary>
        ///// UI Thread
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //private static async Task<BitmapImage> GetCachedCover(string path)
        //{
        //    BitmapImage image = new BitmapImage();
        //    if (path == "") return image;//!
        //    Uri uri = new Uri(path);

        //    var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
        //    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
        //    {
        //        await image.SetSourceAsync(stream);
        //    }

        //    return image;
        //}

        ///// <summary>
        ///// UI Thread
        ///// </summary>
        ///// <param name="album"></param>
        ///// <returns></returns>
        //public static async Task<string> GetAlbumCoverPath(AlbumItem album)
        //{
        //    string imagePath = AppConstants.AlbumCover;
        //    if (!album.IsImageSet)
        //    {
        //        var songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(album.AlbumParam, album.AlbumArtist);
        //        var song = songs.FirstOrDefault(s => s.IsAlbumArtSet && s.CoverPath != AppConstants.AlbumCover);
        //        if (song == null)
        //        {
        //            foreach (var s in songs.Where(x => !x.IsAlbumArtSet))
        //            {
        //                await SaveAlbumArtFromSong(s);
        //                if (s.CoverPath != AppConstants.AlbumCover)
        //                {
        //                    imagePath = s.CoverPath;
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            imagePath = song.CoverPath;
        //        }
        //    }
        //    else
        //    {
        //        imagePath = album.ImagePath;
        //    }

        //    return imagePath;
        //}

        ///// <summary>
        ///// UI Thread
        ///// Search for album art in tags, folder, thumbnail
        ///// If no album art is find return null
        ///// </summary>
        ///// <param name="path">Path to file</param>
        ///// <returns></returns>
        //public static async Task<WriteableBitmap> GetAlbumArtBitmap(string path)
        //{
        //    try
        //    {
        //        int picturesCount = 0;
        //        WriteableBitmap bitmap = new WriteableBitmap(1, 1);

        //        try
        //        {
        //            //Stopwatch st = new Stopwatch();
        //            //st.Start();
        //            StorageFile songFile = await StorageFile.GetFileFromPathAsync(path);
        //            Stream fileStream = await songFile.OpenStreamForReadAsync();
        //            TagLib.File tagFile = TagLib.File.Create(new StreamFileAbstraction(songFile.Name, fileStream, fileStream));
        //            picturesCount = tagFile.Tag.Pictures.Length;
        //            fileStream.Dispose();
        //            if (picturesCount > 0)
        //            {
        //                IPicture pic = tagFile.Tag.Pictures[0];
        //                using (MemoryStream stream = new MemoryStream(pic.Data.Data))
        //                {
        //                    using (IRandomAccessStream istream = stream.AsRandomAccessStream())
        //                    {
        //                        bitmap = await WriteableBitmapExtensions.FromStream(new WriteableBitmap(1, 1), istream);
        //                    }
        //                }
        //                //st.Stop();
        //                //Debug.WriteLine("Tag {0}ms", st.ElapsedMilliseconds);
        //            }
        //            else
        //            {
        //                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
        //                try
        //                {
        //                    // not  exist
        //                    // 15ms 30 ms
        //                    QueryOptions q = new QueryOptions(CommonFileQuery.DefaultQuery, new List<string>() { ".jpg", ".png", ".jpeg" });
        //                    var res = folder.CreateFileQueryWithOptions(q);
        //                    var files2 = await res.GetFilesAsync();
        //                    if (files2.Count > 0)
        //                    {
        //                        //Stopwatch s = new Stopwatch();
        //                        //s.Start();
        //                        //using (IRandomAccessStream istream = await files2.FirstOrDefault().OpenAsync(FileAccessMode.Read))
        //                        //{
        //                        //    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
        //                        //    bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
        //                        //    istream.Seek(0);
        //                        //    bitmap.SetSource(istream);
        //                        //}
        //                        //s.Stop();
        //                        //var b = s.ElapsedMilliseconds;
        //                        //s.Restart();
        //                        using (IRandomAccessStream istream = await files2.FirstOrDefault().OpenAsync(FileAccessMode.Read))
        //                        {
        //                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
        //                            bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
        //                            istream.Seek(0);
        //                            await bitmap.SetSourceAsync(istream);
        //                        }
        //                        //s.Stop();
        //                        //Debug.WriteLine("Folder 1 {0}ms 2 {1}ms", b, s.ElapsedMilliseconds);
        //                    }
        //                    //25ms 50ms
        //                    //IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
        //                    //var file = files.FirstOrDefault(f => f.Path.EndsWith("jpg") || f.Path.EndsWith("png") || f.Path.EndsWith("jpeg"));
        //                    //if (file != null)
        //                    //{
        //                    //    using (IRandomAccessStream istream = await file.OpenAsync(FileAccessMode.Read)) //5-20ms
        //                    //    {
        //                    //        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
        //                    //        bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
        //                    //        istream.Seek(0);
        //                    //        bitmap.SetSource(istream);
        //                    //    }
        //                    //}
        //                }
        //                catch (Exception e)
        //                {

        //                }
        //                //st.Stop();
        //                //Debug.WriteLine("Folder {0}ms", st.ElapsedMilliseconds);
        //            }
        //            if (bitmap.PixelHeight == 1)
        //            {
        //                //Stopwatch s = new Stopwatch();
        //                //s.Start();
        //                try // <5ms
        //                {
        //                    var thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
        //                    if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
        //                    {
        //                        using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
        //                        {
        //                            bitmap = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
        //                            istream.Seek(0);
        //                            bitmap.SetSource(istream);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {

        //                }
        //                //s.Stop();
        //                //Debug.WriteLine("Thumb {0}ms", s.ElapsedMilliseconds);
        //            }
        //            //else
        //            //{
        //            //    var thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
        //            //    if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
        //            //    {
        //            //        Debug.WriteLine("Sizes Tag: {0}x{1} Thumb: {2}x{3}", bitmap.PixelHeight, bitmap.PixelWidth, thumb.OriginalHeight, thumb.OriginalWidth);
        //            //    }
        //            //    else
        //            //    {
        //            //        Debug.WriteLine("no thumbnail");
        //            //    }
        //            //}

        //        }
        //        catch (Exception e)
        //        {

        //        }
        //        return bitmap;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return null;
        //}
        #endregion
    }
}
