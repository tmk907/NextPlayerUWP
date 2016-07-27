using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.Converters
{
    public class BoolToHighlightedItemForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return new SolidColorBrush(Windows.UI.Colors.White);
            }
            else
            {
                if ((string)parameter == "1")
                {
                    if (App.IsLightThemeOn)
                    {
                        var dict = App.Current.Resources.ThemeDictionaries["Light"] as Windows.UI.Xaml.ResourceDictionary;
                        return dict["ListItemForegroundSolid"];
                    }
                    else
                    {
                        var dict = App.Current.Resources.ThemeDictionaries["Dark"] as Windows.UI.Xaml.ResourceDictionary;
                        return dict["ListItemForegroundSolid"];
                    }
                }
                else
                {
                    if (App.IsLightThemeOn)
                    {
                        var dict = App.Current.Resources.ThemeDictionaries["Light"] as Windows.UI.Xaml.ResourceDictionary;
                        return dict["ListItemForegroundLight"];
                    }
                    else
                    {
                        var dict = App.Current.Resources.ThemeDictionaries["Dark"] as Windows.UI.Xaml.ResourceDictionary;
                        return dict["ListItemForegroundLight"];
                    }
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
