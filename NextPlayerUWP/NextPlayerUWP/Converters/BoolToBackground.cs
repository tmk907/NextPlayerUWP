using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
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
                return App.Current.Resources["UserAccentBrush"] as SolidColorBrush;
                //SolidColorBrush brush;
                //if ((bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme))
                //{
                //    brush = App.Current.Resources["UserAccentBrush1Lighter"] as SolidColorBrush;
                //}
                //else
                //{
                //    brush = App.Current.Resources["UserAccentBrush1Darker"] as SolidColorBrush;
                //}
                //return brush;
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
