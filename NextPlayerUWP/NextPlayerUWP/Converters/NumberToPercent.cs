using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class NumberToPercent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value.ToString().Remove(value.ToString().Length - 1);
        }
    }
}
