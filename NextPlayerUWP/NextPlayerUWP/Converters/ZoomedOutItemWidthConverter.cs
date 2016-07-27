using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class ZoomedOutItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string param = parameter as string;
            string val = value as string;
            if (val == "no")
            {
                if (param == "mobile") return 48;
                else return 84;
            }
            else if (val == "duration")
            {
                if (param == "mobile") return 96;
                else return 128;
            }
            else if (val == "date")
            {
                if (param == "mobile") return 144;
                else return 208;
            }
            else
            {
                if (param == "mobile") return 48;
                else return 84;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
