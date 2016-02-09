using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class ThumbToolTipValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan currentTime = TimeSpan.FromSeconds((double)value);

            if (currentTime.CompareTo(TimeSpan.Zero) == -1)
            {
                return "0:00";
            }
            string formatted = "";
            if (currentTime.Hours == 0)
            {
                if (currentTime.Duration().Minutes == 0) formatted = "0" + currentTime.ToString(@"\:ss");
                else formatted = currentTime.ToString(@"m\:ss");
            }
            else
            {
                formatted = currentTime.ToString(@"h\:mm\:ss");
            }
            return formatted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
