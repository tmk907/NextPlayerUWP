using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class VolumeSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((int)value == 0)
            {
                return Symbol.Mute;
            }
            else return Symbol.Volume;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
