using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;

namespace NextPlayerUWP.Common
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
                imagePath = "ms-appx:///Assets/AppImages/Logo/Logo.png";
            }
            return imagePath;
        }
    }
}
