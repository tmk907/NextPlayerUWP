using NextPlayerWinUIDataLayer.Helpers;
using System;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
{
    public class RepeatContent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            RepeatEnum repeat = (RepeatEnum)value;
            return Repeat.GetContent(repeat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
