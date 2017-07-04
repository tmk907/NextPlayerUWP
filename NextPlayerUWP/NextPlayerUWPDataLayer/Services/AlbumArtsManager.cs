using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Images.PaletteUWP;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;

namespace NextPlayerUWPDataLayer.Services
{
    public class AlbumArtsManager
    {
        private const string albumArtsFolderName = "AlbumArts";       

        public async Task SaveAlbumArtAndColor(SongData songData, bool searchInThumbnail = false)
        {
            try
            {
                StorageFile songFile;
                try
                {
                    songFile = await StorageFile.GetFileFromPathAsync(songData.Path);
                }
                catch (Exception ex)
                {
                    songFile = await FutureAccessHelper.GetFileFromPathAsync(songData.Path);
                }
                bool set = false;
                if (searchInThumbnail)
                {
                    set = await SaveBigFromThumbnail(songFile, songData);
                }
                if (!set)
                {
                    try
                    {
                        set = await SaveFromTags(songFile, songData);
                    }
                    catch (Exception ex) { }
                    if (!set)
                    {
                        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(songData.Path));
                        set = await SaveFromFolder(folder, songData);
                    }
                }
                if (!set && searchInThumbnail)
                {
                    set = await SaveAnyFromThumbnail(songFile, songData);
                }
            }
            catch (Exception ex) { }
        }

        public async Task SaveAlbumArtAndColor(StorageFile songFile, SongData songData, bool searchInThumbnail = false)
        {
            try
            {
                bool set = false;
                if (searchInThumbnail)
                {
                    set = await SaveBigFromThumbnail(songFile, songData);
                }
                if (!set)
                {
                    try
                    {
                        set = await SaveFromTags(songFile, songData);
                    }
                    catch (Exception ex) { }
                    if (!set)
                    {
                        StorageFolder folder = await songFile.GetParentAsync();
                        set = await SaveFromFolder(folder, songData);
                    }
                }
                if (!set && searchInThumbnail)
                {
                    set = await SaveAnyFromThumbnail(songFile, songData);
                }
            }
            catch (Exception ex) { }
        }

        public async Task<bool> SaveFromTags(StorageFile file, SongData songData)
        {
            bool saved = false;
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
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                        await SaveAlbumArtAndColor(decoder, songData);
                        saved = true;
                    }
                }
            }
            return saved;
        }

        private async Task<bool> SaveFromFolder(StorageFolder folder, SongData songData)
        {
            bool saved = false;
            try
            {               
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
                        await SaveAlbumArtAndColor(decoder, songData);
                        saved = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return saved;
        }

        private async Task<bool> SaveBigFromThumbnail(StorageFile file, SongData songData)
        {
            bool saved = false;
            var thumb = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
            if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image)
            {
                if (thumb.OriginalWidth > 192 || thumb.OriginalHeight > 192)
                {
                    using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                        await SaveAlbumArtAndColor(decoder, songData);
                        saved = true;
                    }
                }
            }
            return saved;
        }

        private async Task<bool> SaveAnyFromThumbnail(StorageFile file, SongData songData)
        {
            bool saved = false;
            var thumb = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView, 256);
            if (thumb != null && thumb.Type == Windows.Storage.FileProperties.ThumbnailType.Image && (thumb.OriginalHeight > 48 || thumb.OriginalWidth > 48))
            {
                using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                    await SaveAlbumArtAndColor(decoder, songData);
                    saved = true;
                }
            }
            return saved;
        }

        private async Task SaveAlbumArtAndColor(BitmapDecoder decoder, SongData songData)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            var dominant = await paletteHelper.GetColorFromImage(decoder);
            int color = ColorHelpers.ColorToInt(dominant);
            try
            {
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                string outputFileName = GetFileName(songData, color);
                string savedPath = await ImagesManager.SaveCover(outputFileName, albumArtsFolderName, softwareBitmap);
                songData.AlbumArtPath = savedPath;
            }
            catch (Exception ex)
            {

            }
        }

        public static string GetFileName(SongData sd, int color)
        {
            StringBuilder sb = new StringBuilder(sd.Tag.Album.Length + sd.Tag.Artists.Length + sd.Tag.Title.Length);
            sb.Append(sd.Tag.Album).Append('\t').Append(sd.Tag.Artists).Append('\t').Append(sd.Tag.Title);
                
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(sb.ToString());
            var hash = md5.ComputeHash(inputBytes);
            sb.Clear();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            sb.Append('+').Append(color);

            var fileName = sb.ToString();

            return fileName;
        }

    }
}
