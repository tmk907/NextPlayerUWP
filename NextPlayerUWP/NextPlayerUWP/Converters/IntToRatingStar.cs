using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
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
