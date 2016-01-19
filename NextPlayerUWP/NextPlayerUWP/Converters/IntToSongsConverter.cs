using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace NextPlayer.Converters
{
    public class IntToSongsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int number = (int) value;
            return number + " songs";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = value as string;
            strValue.Replace(" songs", "");
            return Int32.Parse(strValue);
            throw new Exception("Unable to convert string to date time");
        }
    }
}
