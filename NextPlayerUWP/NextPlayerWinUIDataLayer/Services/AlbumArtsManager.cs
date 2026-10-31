using NextPlayerWinUIDataLayer.Diagnostics;
using NextPlayerWinUIDataLayer.Helpers;
using NextPlayerWinUIDataLayer.Images.PaletteUWP;
using NextPlayerWinUIDataLayer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace NextPlayerWinUIDataLayer.Services
{
    public class AlbumArtsManager
    {
        private const string albumArtsFolderName = "AlbumArts";
        private const int MaxConcurrentFiles = 4;
        private static readonly TimeSpan ArtTimeout = TimeSpan.FromSeconds(20);
        private static readonly SemaphoreSlim artLimiter = new SemaphoreSlim(MaxConcurrentFiles, MaxConcurrentFiles);

        public Task SaveAlbumArtAndColor(SongData songData, bool searchInThumbnail = false)
        {
            return RunGuardedAsync(songData, async () =>
            {
                StorageFile songFile;
                try
                {
                    songFile = await StorageFile.GetFileFromPathAsync(songData.Path);
                }
                catch (Exception)
                {
                    songFile = await FutureAccessHelper.GetFileFromPathAsync(songData.Path);
                }
                await FindAndSaveAsync(songFile, songData, searchInThumbnail);
            });
        }

        public Task SaveAlbumArtAndColor(StorageFile songFile, SongData songData, bool searchInThumbnail = false)
        {
            return RunGuardedAsync(songData, () => FindAndSaveAsync(songFile, songData, searchInThumbnail));
        }

        /// <summary>
        /// A library scan handles many files in parallel, and in a desktop process decoding a shell thumbnail
        /// can leave its async operation unfinished forever. One such file must not stall the whole scan,
        /// so limit concurrency and stop waiting for a file after a while: it just ends up without a cover.
        /// </summary>
        private static async Task RunGuardedAsync(SongData songData, Func<Task> save)
        {
            await artLimiter.WaitAsync();
            try
            {
                var saving = save();
                if (await Task.WhenAny(saving, Task.Delay(ArtTimeout)) == saving)
                {
                    await saving;
                }
                else
                {
                    Logger2.Current.WriteMessage("Album art timeout " + songData.Path, Logger2.Level.WarningError);
                }
            }
            catch (Exception ex)
            {
                Logger2.Current.WriteMessage("Album art error " + songData.Path + " " + ex.Message, Logger2.Level.WarningError);
            }
            finally
            {
                artLimiter.Release();
            }
        }

        private async Task FindAndSaveAsync(StorageFile songFile, SongData songData, bool searchInThumbnail)
        {
            bool set = false;
            try
            {
                set = await SaveFromTags(songFile, songData);
            }
            catch (Exception) { }

            if (!set)
            {
                StorageFolder folder = await songFile.GetParentAsync();
                set = await SaveFromFolder(folder, songData);
            }
            if (!set && searchInThumbnail)
            {
                await SaveFromThumbnail(songFile, songData);
            }
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

        private async Task<bool> SaveFromThumbnail(StorageFile file, SongData songData)
        {
            using (var thumb = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 256))
            {
                if (thumb == null || thumb.Type != ThumbnailType.Image || (thumb.OriginalHeight <= 48 && thumb.OriginalWidth <= 48))
                {
                    return false;
                }
                using (var istream = thumb.AsStreamForRead().AsRandomAccessStream())
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                    await SaveAlbumArtAndColor(decoder, songData);
                    return true;
                }
            }
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

        public async Task SaveAlbumArtAndColor(WriteableBitmap bitmap, SongData songData)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            var dominant = paletteHelper.GetColorFromImage(bitmap);
            int color = ColorHelpers.ColorToInt(dominant);
            try
            {
                string outputFileName = GetFileName(songData, color);
                string savedPath = await ImagesManager.SaveCover(outputFileName, albumArtsFolderName, bitmap);
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
