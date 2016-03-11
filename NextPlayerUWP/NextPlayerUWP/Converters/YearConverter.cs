using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class YearConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (((int)value) == 0) return "";
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
