using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace NextPlayerUWPDataLayer.Services
{
    public class ImagesManager
    {
        public static async Task<string> SaveAlbumCover(string album, string tileId)
        {
            var songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(album);
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
        /// </summary>
        public static async Task<BitmapImage> GetOriginalCover(string path, bool small)
        {
            BitmapImage bitmap = new BitmapImage();
            if (small)
            {
                bitmap.DecodePixelHeight = 192;
            }
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
                        using (Windows.Storage.Streams.IRandomAccessStream istream = stream.AsRandomAccessStream())
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
                                    using (IRandomAccessStream stream = await x.OpenAsync(Windows.Storage.FileAccessMode.Read))
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
        /// </summary>
        public static async Task<BitmapImage> GetDefaultCover(bool small)
        {
            BitmapImage bitmap = new BitmapImage();
            Uri uri;
            if (small)
            {
                //if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-light192.png");
                //}
                //else
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-dark192.png");
                //}
                uri = new System.Uri("ms-appx:///Assets/SongCover192.png");
            }
            else
            {
                //if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-light.png");
                //}
                //else
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-dark.png");
                //}
                uri = new System.Uri("ms-appx:///Assets/SongCover.png");
            }
            var file2 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            using (IRandomAccessStream stream = await file2.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                await bitmap.SetSourceAsync(stream);
            }
            return bitmap;
        }

        /// <summary>
        /// Return cover saved in file tags or .jpg from folder or default cover.
        /// </summary>
        public static async Task<BitmapImage> GetCover(string path, bool small)
        {
            BitmapImage bitmap = new BitmapImage();

            bitmap = await GetOriginalCover(path, small);

            if (bitmap.PixelHeight == 0)
            {
                bitmap = await GetDefaultCover(small);
            }

            return bitmap;
        }

        public static async Task<BitmapImage> GetCurrentCover(int index)
        {
            return await GetCover(NowPlayingPlaylistManager.Current.GetSongItem(index).Path, false);
        }



    }
}
