using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace NextPlayerWinUIDataLayer.Services
{
    public static class WriteableBitmapExtensions
    {
        public static async Task<WriteableBitmap> ResizeAsync(this WriteableBitmap originalBitmap, int newWidth, int newHeight)
        {
            // Get the pixel buffer from the original WriteableBitmap
            using (var pixelStream = originalBitmap.PixelBuffer.AsStream())
            {
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                // Create a new WriteableBitmap with the desired dimensions
                var resizedBitmap = new WriteableBitmap(newWidth, newHeight);

                // Use a BitmapEncoder to scale the image
                using (var stream = new InMemoryRandomAccessStream())
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied,
                        (uint)originalBitmap.PixelWidth,
                        (uint)originalBitmap.PixelHeight,
                        96,
                        96,
                        pixels);

                    // Scale the image
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Cubic;
                    encoder.BitmapTransform.ScaledWidth = (uint)newWidth;
                    encoder.BitmapTransform.ScaledHeight = (uint)newHeight;
                    await encoder.FlushAsync();

                    // Decode the scaled image into the new WriteableBitmap
                    stream.Seek(0);
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var pixelData = await decoder.GetPixelDataAsync();
                    var resizedPixels = pixelData.DetachPixelData();

                    using (var resizedStream = resizedBitmap.PixelBuffer.AsStream())
                    {
                        await resizedStream.WriteAsync(resizedPixels, 0, resizedPixels.Length);
                    }
                }

                return resizedBitmap;
            }
        }
    }
}