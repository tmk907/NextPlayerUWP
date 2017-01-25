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
using Windows.UI;
using NextPlayerUWPDataLayer.Helpers;

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
            var bmp = await GetAlbumArtSoftwareBitmap(song.Path, true);
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
            int width = 0;
            int height = 0;
            if (resize != 0)
            {
                width = image.PixelWidth;
                height = image.PixelHeight;
                if (width == height)
                {
                    height = resize;
                }
                else
                {
                    height = height * resize / width;
                }
                width = resize;
            }
            try
            {
                await SaveBitmap(file, image, height, width);
            }
            catch (UnauthorizedAccessException ex)
            {
                //image exists and is opened in album view
                try
                {
                    file = await folder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.GenerateUniqueName);
                    await SaveBitmap(file, image, height, width);
                }
                catch(Exception ex2)
                {

                }
            }
            catch(Exception ex)
            {

            }
            return "ms-appdata:///local/" + folderName + "/" + file.Name;
        }

        public static async Task<string> SaveCover(string fileName, string folderName, SoftwareBitmap image, int resize = 0)
        {
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            StorageFile file = await folder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);

            int width = 0;
            int height = 0;
            if (resize != 0)
            {
                width = image.PixelWidth;
                height = image.PixelHeight;
                if (width == height)
                {
                    height = resize;
                }
                else
                {
                    height = height * resize / width;
                }
                width = resize;
            }
            try
            {
                await SaveBitmap(file, image, (uint)width, (uint)height);
            }
            catch (UnauthorizedAccessException ex)
            {
                //image exists and is opened in album view
                try
                {
                    file = await folder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.GenerateUniqueName);
                    await SaveBitmap(file, image, (uint)height, (uint)width);
                }
                catch (Exception ex2)
                {

                }
            }
            catch (Exception ex)
            {

            }

            return "ms-appdata:///local/" + folderName + "/" + file.Name;
        }

        public static async Task SaveBitmap(StorageFile file, SoftwareBitmap bitmap, uint newHeight = 0, uint newWidth = 0)
        {
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(bitmap);
                if (newWidth > 0 || newHeight > 0)
                {
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                    encoder.BitmapTransform.ScaledHeight = newHeight;
                    encoder.BitmapTransform.ScaledWidth = newWidth;
                }
                encoder.IsThumbnailGenerated = true;
                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    switch (err.HResult)
                    {
                        case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                         // If the encoder does not support writing a thumbnail, then try again
                                                         // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw err;
                    }
                }
                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }
            }
        }

        /// <summary>
        /// UI Thread
        /// </summary>
        /// <param name="file"></param>
        /// <param name="bitmap"></param>
        /// <param name="newHeight"></param>
        /// <param name="newWidth"></param>
        /// <returns></returns>
        public static async Task SaveBitmap(StorageFile file, WriteableBitmap bitmap, int newHeight = 0, int newWidth = 0)
        {
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                if (newWidth > 0 || newHeight > 0)
                {
                    bitmap = bitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                }

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;

                using (var pixelStream = bitmap.PixelBuffer.AsStream())
                {
                    byte[] pixels = new byte[bitmap.PixelBuffer.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, pixels);
                    await encoder.FlushAsync();
                }
            }
        }

        public static async Task SaveAlbumArtFromSong(SongItem song)
        {
            var cover = await GetAlbumArtSoftwareBitmap(song.Path);

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
            var cover = await GetAlbumArtSoftwareBitmap(song.Path, true);

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

        public static async Task TryDeleteAppLocalFile(string filePath)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(filePath));
            await file.DeleteAsync();
        }

        /// <summary>
        /// Search for album art in thumbnail, tags, folder
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>bitmap or null if no album art is found</returns>
        public static async Task<SoftwareBitmap> GetAlbumArtSoftwareBitmap(string path, bool searchInThumbnail = false)
        {
            try
            {
                SoftwareBitmap softwareBitmap = null;
                StorageFile songFile;
                try
                {
                    songFile = await StorageFile.GetFileFromPathAsync(path);
                }
                catch (Exception ex)
                {
                    songFile = await FutureAccessHelper.GetFileFromPathAsync(path);
                    if (songFile == null) throw;
                }
                bool set = false;
                // <5ms
                var thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
                if (searchInThumbnail && thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
                {
                    if (thumb.OriginalWidth > 192 || thumb.OriginalHeight > 192)
                    {
                        using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                        {
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                            softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                        }
                        set = true;
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
                                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();
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
                                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                                }
                                set = true;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                if (!set && searchInThumbnail)
                {
                    thumb = await songFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView, 256);
                    if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image && (thumb.OriginalHeight > 48 || thumb.OriginalWidth > 48))
                    {
                        using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                        {
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                            softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                        }
                        set = true;
                    }
                }
                if (set) return softwareBitmap;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
