using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace NextPlayerUWP.Common.Tiles
{
    public class TileImageHelper
    {
        const int maxSize = 800;
        const int targetSize = 512;

        public async Task<string> PrepareTileImage(string coverUri)
        {
            if (coverUri.StartsWith("ms-appx")) return coverUri;

            SoftwareBitmap softwareBitmap;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(coverUri));
            using (IRandomAccessStream istream = await file.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            
            int width = softwareBitmap.PixelWidth;
            int height = softwareBitmap.PixelHeight;
            string newCoverUri = coverUri;
            if (width > maxSize || height > maxSize)
            {
                if (width == height)
                {
                    height = targetSize;
                    width = targetSize;
                }
                else if (width > height)
                {
                    height = height * targetSize / width;
                    width = targetSize;
                }
                else
                {
                    width = width * targetSize / height;
                    height = targetSize;
                }

                var folder = ApplicationData.Current.TemporaryFolder;
                string newFilename = coverUri.Substring(coverUri.LastIndexOf('/') + 1);
                newCoverUri = "ms-appdata:///temp/" + newFilename;
                StorageFile outputFile = await folder.CreateFileAsync(newFilename, CreationCollisionOption.ReplaceExisting);

                await SaveBitmap(outputFile, softwareBitmap, (uint)height, (uint)width);
            }
            return newCoverUri;
        }

        private async Task SaveBitmap(StorageFile file, SoftwareBitmap bitmap, uint newHeight = 0, uint newWidth = 0)
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
    }
}
