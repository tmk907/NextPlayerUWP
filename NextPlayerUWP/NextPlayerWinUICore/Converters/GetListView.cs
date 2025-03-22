using System;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
{
    public class GetListView : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
