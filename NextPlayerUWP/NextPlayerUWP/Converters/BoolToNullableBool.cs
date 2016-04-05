using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class BoolToNullableBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null) return value;
            else return false;
        }
    }
}
