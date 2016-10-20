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

        public async Task<string> PrepareImage(string coverUri)
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
                string newFilename = coverUri.Substring(coverUri.LastIndexOf('/') + 1);

                var folder = ApplicationData.Current.TemporaryFolder;
                StorageFile outputFile = await folder.CreateFileAsync(newFilename, CreationCollisionOption.ReplaceExisting);
                using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetSoftwareBitmap(softwareBitmap);

                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                    encoder.BitmapTransform.ScaledHeight = (uint)height;
                    encoder.BitmapTransform.ScaledWidth = (uint)width;
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
            return coverUri;
        }
    }
}
