using System;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
{
    public class PlaylistNameToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string name = value as string;
            if (name==null || name == "")
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
