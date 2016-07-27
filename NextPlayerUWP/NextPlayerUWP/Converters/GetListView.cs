using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
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
