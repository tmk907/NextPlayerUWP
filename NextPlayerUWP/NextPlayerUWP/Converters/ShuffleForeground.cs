using NextPlayerUWPDataLayer.Helpers;
using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class ShuffleForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool adjust = false;
            if (parameter != null) adjust = Boolean.Parse(parameter.ToString());
            bool shuffle = (bool)value;
            return Shuffle.GetColor(shuffle, adjust);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
