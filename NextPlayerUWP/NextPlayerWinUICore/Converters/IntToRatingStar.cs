using System;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
{
    public class IntToRatingStar : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int rating = (int)value;
            int param = Int32.Parse(parameter.ToString());
            if (param <= rating)
            {
                return "\uE1CF";
            }
            else
            {
                return "\uE1CE";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
