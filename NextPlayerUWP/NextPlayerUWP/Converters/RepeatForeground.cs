using NextPlayerUWPDataLayer.Helpers;
using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class RepeatForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            RepeatEnum repeat = (RepeatEnum)value;
            return Repeat.GetColor(repeat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
