using NextPlayerUWPDataLayer.Helpers;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.AppColors
{
    public class AppAccentColors
    {
        private readonly string[] Win10HexAccentColors = new string[] {
            "#FFFFB900", "#FFE74856", "#FF0078D7", "#FF0099BC", "#FF7A7574", "#FF767676",
            "#FFFF8C00", "#FFE81123", "#FF0063B1", "#FF2D7D9A", "#FF5D5A58", "#FF4C4A48",
            "#FFF7630C", "#FFEA005E", "#FF8E8CD8", "#FF00B7C3", "#FF68768A", "#FF69797E",
            "#FFCA5010", "#FFC30052", "#FF6B69D6", "#FF038387", "#FF515C6B", "#FF4A5459",
            "#FFDA3B01", "#FFE3008C", "#FF8764B8", "#FF00B294", "#FF567C73", "#FF647C64",
            "#FFEF6950", "#FFBF0077", "#FF744DA9", "#FF018574", "#FF486860", "#FF525E54",
            "#FFD13438", "#FFC239B3", "#FFB146C2", "#FF00CC6A", "#FF498205", "#FF847545",
            "#FFFF4343", "#FF9A0089", "#FF881798", "#FF10893E", "#FF107C10", "#FF7E735F"
        };

        public Color[] GetWin10AccentColors()
        {
            Color[] colors = new Color[Win10HexAccentColors.Length];
            for (int i = 0; i < Win10HexAccentColors.Length; i++)
            {
                byte a = byte.Parse(Win10HexAccentColors[i].Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte r = byte.Parse(Win10HexAccentColors[i].Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(Win10HexAccentColors[i].Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(Win10HexAccentColors[i].Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                colors[i] = Color.FromArgb(a, r, g, b);
            }
            return colors;
        }

        public void SetAccentColorShades(Color color)
        {
            //var v = (hsv.Item3 < 0.70) ? (hsv.Item3 + 0.3) : 1.0;
            ColorConverter cc = new ColorConverter();
            var hsl = cc.ColorToHSL(color);
            var l = (hsl.Item3 < 0.9) ? (hsl.Item3 + 0.05) : 0.9;
            var d = (hsl.Item3 > 0.1) ? (hsl.Item3 - 0.05) : 0.1;

            var lighter1 = cc.HSLToColor(hsl.Item1, hsl.Item2, l);
            var darker1 = cc.HSLToColor(hsl.Item1, hsl.Item2, d);

            //UISettings s = new UISettings();
            ((SolidColorBrush)App.Current.Resources["UserAccentBrush1Lighter"]).Color = lighter1;
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush2Lighter"]).Color = s.GetColorValue(UIColorType.AccentLight2);
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush3Lighter"]).Color = s.GetColorValue(UIColorType.AccentLight3);
            ((SolidColorBrush)App.Current.Resources["UserAccentBrush1Darker"]).Color = darker1;
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush2Darker"]).Color = s.GetColorValue(UIColorType.AccentDark2);
            //((SolidColorBrush)App.Current.Resources["UserAccentBrush3Darker"]).Color = s.GetColorValue(UIColorType.AccentDark3);
        }

        public bool IsColorDark(Color c)
        {
            bool colorIsDark = (5 * c.G + 2 * c.R + c.B) <= 8 * 128;
            return colorIsDark;
        }

        public static Color GetSavedAppAccentColor()
        {
            string hexColor = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppAccent) as string;
            if (hexColor == null) return Windows.UI.Color.FromArgb(255, 0, 120, 215);
            byte a = byte.Parse(hexColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            byte r = byte.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
            Color color = Color.FromArgb(a, r, g, b);
            return color;
        }

        public static void RestoreAppAccentColors()
        {
            var accent = GetSavedAppAccentColor();
            ChangeAppAccentColor(accent, accent);          
        }
 
        public static void ChangeAppAccentColor(Color color, Color albumArtAccent)
        {
            foreach (var dict in App.Current.Resources.ThemeDictionaries)
            {
                var theme = dict.Value as Windows.UI.Xaml.ResourceDictionary;

                ((SolidColorBrush)theme["UserAccentBrush"]).Color = color;
                ((SolidColorBrush)theme["AlbumArtAccentBrush"]).Color = albumArtAccent;

                byte transparency = 128;
                if (dict.Key as string == "Dark")
                {
                    transparency = 200;
                }
                var colorTr = Color.FromArgb(transparency, color.R, color.G, color.B);
                ((SolidColorBrush)theme["UserAccentBrushTr"]).Color = colorTr;

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

        public static void SaveAppAccentColor(Color color)
        {
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AppAccent, color.ToString());
        }
    }
}
