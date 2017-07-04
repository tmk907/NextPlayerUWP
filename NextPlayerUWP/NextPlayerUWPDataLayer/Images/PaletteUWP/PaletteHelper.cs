using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace NextPlayerUWPDataLayer.Images.PaletteUWP
{
    public class PaletteHelper
    {
        public async Task<Color> GetColorFromLocalAlbumArt(Uri albumUri)
        {
            Palette p;
            //WriteableBitmap wb = await new WriteableBitmap(1, 1).FromContent(albumUri);
            //p = Palette.From(wb).Generate();

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(albumUri);
            using (IRandomAccessStream istream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                p = await Palette.From(decoder).Generate();
            }
            return GetColorFromPalette(p);
        }

        public Color GetColorFromImage(WriteableBitmap wb)
        {
            Palette p = Palette.From(wb).Generate();
            return GetColorFromPalette(p);
        }

        public async Task<Color> GetColorFromImage(StorageFile file)
        {
            Palette p;
            using (IRandomAccessStream istream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                p = await Palette.From(decoder).Generate();
            }
            return GetColorFromPalette(p);
        }

        public async Task<Color> GetColorFromImage(BitmapDecoder decoder)
        {
            Palette p = await Palette.From(decoder).Generate();
            return GetColorFromPalette(p);
        }

        private Color GetColorFromPalette(Palette p)
        {
            int color;
            var darkVibrant = p.GetDarkVibrantColor(-1);
            var vibrant = p.GetVibrantColor(-1);
            //var c3 = p.GetLightVibrantColor(-1);
            var darkMuted = p.GetDarkMutedColor(-1);
            //var c5 = p.GetMutedColor(-1);
            //var c6 = p.GetLightMutedColor(-1);
            var dominant = p.GetDominantColor(-1);
            float[] h1 = new float[3];
            float[] h2 = new float[3];
            ColorHelpers.ColorToHSL(darkVibrant, h1);
            ColorHelpers.ColorToHSL(vibrant, h2);
            int darker = WhichIsDarker(h1, h2);
            if (darker == 0)
            {
                color = darkVibrant;
            }
            else if (darker == 1)
            {
                color = vibrant;
            }
            else
            {
                color = darkMuted;
            }
            if (color == -1)
            {
                color = dominant;
            }
            //Debug.WriteLine("Color {0} {1} {2} {3} {4} {5}", color, darkVibrant, vibrant, darkMuted, dominant, albumUri);

            return ColorHelpers.ColorFromInt(color);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hsl1"></param>
        /// <param name="hsl2"></param>
        /// <returns>0 hsl1, 1 hsl2, 2 both are white</returns>
        private int WhichIsDarker(float[] hsl1, float[] hsl2)
        {
            int i = 0;

            // one is white
            if (hsl1[0] == 0 && hsl1[1] == 0 && hsl1[2] == 1)
            {
                if (hsl2[0] == 0 && hsl2[1] == 0 && hsl2[2] == 1) return 2;
                else return 1;
            }
            else if (hsl2[0] == 0 && hsl2[1] == 0 && hsl2[2] == 1) return 0;

            if (hsl1[1] < hsl2[1] && hsl1[2] < hsl2[2]) return 0;
            if (hsl1[1] > hsl2[1] && hsl1[2] > hsl2[2]) return 1;

            if (hsl1[2] < hsl2[2]) return 0;
            else return 1;
        }
    }
}
