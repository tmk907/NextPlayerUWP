using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class TimeSpanToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan span = (TimeSpan)value;
            if (span.CompareTo(TimeSpan.Zero) == -1)
            {
                return "0:00";
            }
            string formatted = "";
            if (span.Hours == 0)
            {
                if (span.Duration().Minutes == 0) formatted = "0" + span.ToString(@"\:ss");
                else formatted = span.ToString(@"m\:ss");
            }
            else if (span.Days == 0)
            {
                formatted = span.ToString(@"h\:mm\:ss");
            }
            else
            {
                formatted = span.ToString(@"d\.hh\:mm\:ss");
            }
            return formatted;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = value as string;
            TimeSpan resultSpan;
            if (TimeSpan.TryParse(strValue, out resultSpan))
            {
                return resultSpan;
            }
            throw new Exception("Unable to convert string to date time");
        }
    }
}
