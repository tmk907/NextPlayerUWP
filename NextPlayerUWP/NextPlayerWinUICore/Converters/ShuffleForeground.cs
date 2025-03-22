using NextPlayerWinUIDataLayer.Helpers;
using System;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
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
