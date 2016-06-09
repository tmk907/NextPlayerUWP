using NextPlayerUWP.Common;
using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class ComboBoxItemValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (ComboBoxItemValue)value;
        }
    }
}
