using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.Converters
{
    public class BoolToBackgroundHighlight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                foreach (var dict in App.Current.Resources.ThemeDictionaries)
                {
                    var theme = dict.Value as Windows.UI.Xaml.ResourceDictionary;
                    if (dict.Key as string == "Light" && App.IsLightThemeOn)
                    {
                        var a = theme["AlbumArtAccentBrushTr"] as SolidColorBrush;
                        return a;
                    }
                    else if (dict.Key as string == "Dark" && !App.IsLightThemeOn)
                    {
                        var a = theme["AlbumArtAccentBrushTr"] as SolidColorBrush;
                        return a;
                    }
                }
                return App.Current.Resources["AlbumArtAccentBrushTr"] as SolidColorBrush;
            }
            else
            {
                return new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
