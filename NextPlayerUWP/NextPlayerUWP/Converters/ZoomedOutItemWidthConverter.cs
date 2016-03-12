using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class ZoomedOutItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string val = value as string;
            if (val == "no")
            {
                return 84;
            }
            else if (val == "duration")
            {
                return 128;
            }
            else if (val == "date")
            {
                return 210;
            }
            else
            {
                return 84;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
