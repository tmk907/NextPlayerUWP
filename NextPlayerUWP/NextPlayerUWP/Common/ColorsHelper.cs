using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.Common
{
    public class ColorsHelper
    {
        private string[] Win10HexAccentColors = new string[] {
            "#FFFFB900", "#FFE74856", "#FF0078D7", "#FF0099BC", "#FF767676", "#FF767676",
            "#FFFF8C00", "#FFE81123", "#FF0063B1", "#FF2D7D9A", "#FF5D5A58", "#FF4C4A48",
            "#FFF7630C", "#FFEA005E", "#FF8E8CD8", "#FF00B7C3", "#FF68768A", "#FF69797E",
            "#FFCA5010", "#FFC30052", "#FF6B69D6", "#FF038387", "#FF515C6B", "#FF4A5459",
            "#FFDA3B01", "#FFE3008C", "#FF8764B8", "#FF00B294", "#FF567C73", "#FF647C64",
            "#FFEF6950", "#FFBF0077", "#FF744DA9", "#FF018574", "#FF486860", "#FF525E54",
            "#FFD13438", "#FFC239B3", "#FFB146C2", "#FF00CC6A", "#FF498205", "#FF847545",
            "#FFFF4343", "#FF9A0089", "#FF881798", "#FF10893E", "#FF107C10", "#FF7E735F"
        };

        public void SetAccentColorShades(Color color)
        {
            //var v = (hsv.Item3 < 0.70) ? (hsv.Item3 + 0.3) : 1.0;
            var hsl = ColorToHSL(color);
            var l = (hsl.Item3 < 0.9) ? (hsl.Item3 + 0.05) : 0.9;
            var d = (hsl.Item3 > 0.1) ? (hsl.Item3 - 0.05) : 0.1;

            var lighter1 = HSLToColor(hsl.Item1, hsl.Item2, l);
            var darker1 = HSLToColor(hsl.Item1, hsl.Item2, d);

            //UISettings s = new UISettings();
            ((SolidColorBrush)App.Current.Resources["UserAccentBrush1Lighter"]).Color = lighter1;
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush2Lighter"]).Color = s.GetColorValue(UIColorType.AccentLight2);
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush3Lighter"]).Color = s.GetColorValue(UIColorType.AccentLight3);
            ((SolidColorBrush)App.Current.Resources["UserAccentBrush1Darker"]).Color = darker1;
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush2Darker"]).Color = s.GetColorValue(UIColorType.AccentDark2);
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush3Darker"]).Color = s.GetColorValue(UIColorType.AccentDark3);
        }

        public Color GetSavedUserAccentColor()
        {
            string hexColor = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppAccent) as string;
            if (hexColor == null) return Windows.UI.Color.FromArgb(255, 0, 120, 215);
            byte a = byte.Parse(hexColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            byte r = byte.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
            Color color = Color.FromArgb(a, r, g, b);
            return color;
        }

        public Color[] GetWin10Colors()
        {
            Color[] colors = new Color[48];
            for(int i = 0; i < 48; i++)
            {
                byte a = byte.Parse(Win10HexAccentColors[i].Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte r = byte.Parse(Win10HexAccentColors[i].Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(Win10HexAccentColors[i].Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(Win10HexAccentColors[i].Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                colors[i] = Color.FromArgb(a, r, g, b);
            }
            return colors;
        }

        public void RestoreUserAccentColors()
        {
            var accent = GetSavedUserAccentColor();
            ChangeCurrentAccentColor(accent);          
        }
 
        public void ChangeCurrentAccentColor(Color color)
        {
            foreach (var dict in App.Current.Resources.ThemeDictionaries)
            {
                var theme = dict.Value as Windows.UI.Xaml.ResourceDictionary;
                theme["UserAccentColor"] = color;
                ((SolidColorBrush)theme["UserAccentBrush"]).Color = color;

                ((SolidColorBrush)theme["SystemControlBackgroundAccentBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlDisabledAccentBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlForegroundAccentBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightAccentBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightAltAccentBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHyperlinkTextBrush"]).Color = color;
                ((SolidColorBrush)theme["ContentDialogBorderThemeBrush"]).Color = color;
                ((SolidColorBrush)theme["JumpListDefaultEnabledBackground"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightAltListAccentHighBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightAltListAccentLowBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightAltListAccentMediumBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightListAccentHighBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightListAccentLowBrush"]).Color = color;
                ((SolidColorBrush)theme["SystemControlHighlightListAccentMediumBrush"]).Color = color;
            }

            //SetAccentColorShades(color);
        }

        public void SaveUserAccentColor(Color color)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppAccent, color.ToString());
        }



        public double GetHue(Color color)
        {
            double hue;

            double r = color.R / 255d;
            double g = color.G / 255d;
            double b = color.B / 255d;

            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));

            if (max == 0)
            {
                hue = 0;
                return hue;
            }

            if (r == max)
            {
                hue = (g - b) / (max - min);
            }
            else if (g == max)
            {
                hue = 2.0 + (b - r) / (max - min);
            }
            else
            {
                hue = 4.0 + (r - g) / (max - min);
            }

            hue *= 60;
            if (hue < 0) hue += 360;
            return hue;
        }

        public Tuple<double, double, double> ColorToHSV(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            double hue = GetHue(color);

            double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            double value = max / 255d;

            Tuple<double, double, double> t = new Tuple<double, double, double>(hue, saturation, value);
            return t;
        }

        public Color HSVToColor(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            byte v2 = (byte)v;
            byte p2 = (byte)p;
            byte q2 = (byte)q;
            byte t2 = (byte)t;

            if (hi == 0)
                return Color.FromArgb(255, v2, t2, p2);
            else if (hi == 1)
                return Color.FromArgb(255, q2, v2, p2);
            else if (hi == 2)
                return Color.FromArgb(255, p2, v2, t2);
            else if (hi == 3)
                return Color.FromArgb(255, p2, q2, v2);
            else if (hi == 4)
                return Color.FromArgb(255, t2, p2, v2);
            else
                return Color.FromArgb(255, v2, p2, q2);
        }

        //hue[0,360], saturation[0,1], lightness[0,1]
        public Tuple<double, double, double> ColorToHSL(Color color)
        {
            double r = color.R / 255d;
            double g = color.G / 255d;
            double b = color.B / 255d;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double delta = max - min;

            double hue, saturation, lightness;

            lightness = (max + min) / 2.0;

            if (delta == 0)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                if (lightness < 0.5)
                {
                    saturation = (max - min) / (max + min);
                }
                else
                {
                    saturation = (max - min) / (2.0 - max - min);
                }

                if (max == r)
                {
                    hue = (g - b) / delta;
                }
                else if (max == g)
                {
                    hue = 2.0 + (b - r) / delta;
                }
                else
                {
                    hue = 4.0 + (r - g) / delta;
                }

                hue *= 60;
                if (hue < 0) hue += 360;
            }

            return new Tuple<double, double, double>(hue, saturation, lightness);
        }

        //hue[0,360], saturation[0,1], lightness[0,1]
        public Color HSLToColor(double hue, double saturation, double lightness)
        {
            double red, green, blue;

            if (saturation == 0)
            {
                red = green = blue = lightness * 255; // achromatic
            }
            else
            {
                double temp1;
                if (lightness < 0.5)
                {
                    temp1 = lightness * (1.0 + saturation);
                }
                else
                {
                    temp1 = lightness + saturation - lightness * saturation;
                }
                double temp2 = 2.0 * lightness - temp1;
                hue = hue / 360.0;

                double tempR, tempG, tempB;
                tempR = hue + 1.0 / 3.0;
                tempG = hue;
                tempB = hue - 1.0 / 3.0;

                red = HSLHelper(temp1, temp2, tempR);
                green = HSLHelper(temp1, temp2, tempG);
                blue = HSLHelper(temp1, temp2, tempB);
            }

            return Color.FromArgb(255, (byte)(int)Math.Round(red * 255), (byte)(int)Math.Round(green * 255), (byte)(int)Math.Round(blue * 255));
        }

        private double HSLHelper(double temp1, double temp2, double tempCol)
        {
            double col;
            if (tempCol > 1) tempCol -= 1.0;
            if (tempCol > 1) tempCol += 1.0;
            if (6.0 * tempCol < 1)
            {
                col = temp2 + (temp2 - temp2) * 6.0 * tempCol;
            }
            else if (2.0 * tempCol < 1)
            {
                col = temp1;
            }
            else if (3.0 * tempCol < 2)
            {
                col = temp2 + (temp1 - temp2) * (2.0 / 3.0 - tempCol) * 6.0;
            }
            else
            {
                col = temp2;
            }
            return col;
        }

        private double HueToRGB(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        }
    }
}
