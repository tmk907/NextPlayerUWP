using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class YearToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (((int)value) == 0) return Visibility.Collapsed;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
