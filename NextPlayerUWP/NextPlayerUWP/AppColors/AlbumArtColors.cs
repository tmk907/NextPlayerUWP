using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Images.PaletteUWP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Windows.UI;

namespace NextPlayerUWP.AppColors
{
    public class AlbumArtColors
    {
        private static ConcurrentDictionary<Uri, Color> cachedColors = new ConcurrentDictionary<Uri, Color>();
        public static readonly Uri DefaultAlbumArtUri = new Uri(AppConstants.SongCoverBig);

        private ColorDifference colorDifference;
        private PaletteHelper palette;
        private List<Color> colorsForDark = new List<Color>();
        private List<Color> colorsForLight = new List<Color>();

        public AlbumArtColors()
        {
            colorDifference = new ColorDifference();
            palette = new PaletteHelper();
            AppAccentColors aac = new AppAccentColors();
            var all = aac.GetWin10AccentColors();
            int white = ColorHelpers.ColorToInt(Colors.White);
            int black = ColorHelpers.ColorToInt(Colors.Black);
            foreach (var c in all)
            {
                var contr = ColorHelpers.CalculateContrast(ColorHelpers.ColorToInt(c), white);
                var contr2 = ColorHelpers.CalculateContrast(ColorHelpers.ColorToInt(c), black);
                if (contr >= 4.5)
                {
                    colorsForLight.Add(c);
                }
                if (contr2 >= 4.5)
                {
                    colorsForDark.Add(c);
                }
            }
        }

        public static Uri GetAlbumCoverAssetWithCurrentAccentColor()
        {
            string hexColor = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppAccent) as string;
            return new Uri("ms-appx:///Assets/Albums/Colors/" + hexColor.Substring(1) + "-min.png");
        }

        public Color GetDominantColorFromSavedAlbumArt(Uri albumArtUri)
        {
            Color dominant = AppAccentColors.GetSavedAppAccentColor();
            if (albumArtUri.ToString() != DefaultAlbumArtUri.ToString())
            {
                if (cachedColors.ContainsKey(albumArtUri))
                {
                    dominant = cachedColors[albumArtUri];
                }
                else
                {
                    var parts = albumArtUri.ToString().Split('+');
                    if (parts.Length==2 && !String.IsNullOrWhiteSpace(parts[1]))
                    {
                        string i = parts[1].Replace(".jpg", "");
                        int c = 0;
                        if (Int32.TryParse(i, out c))
                        {
                            dominant = ColorHelpers.ColorFromInt(c);
                            cachedColors.TryAdd(albumArtUri, dominant);
                        }
                    }
                }
            }
            return dominant;
        }

        public Color CreateAppAccentFromAlbumArtColor(Color albumArtColor)
        {
            Color closest = albumArtColor;
            Color bg = Colors.Black;
            var list = colorsForDark;
            if (Common.ThemeHelper.IsLightTheme)
            {
                bg = Colors.White;
                list = colorsForLight;
            }
            double contrast = ColorHelpers.CalculateContrast(ColorHelpers.ColorToInt(albumArtColor), ColorHelpers.ColorToInt(bg));
            if (contrast < 4.5)
            {
                closest = colorDifference.GetClosestColor(albumArtColor, list);
            }
            else
            {
                //return albumArtColor;
            }
            return closest;
        }
    }
}
