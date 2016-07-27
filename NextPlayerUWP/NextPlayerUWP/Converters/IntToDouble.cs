using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class IntToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (double)(int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (int)(double)value;
        }
    }
}
