using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class BoolToInvertedVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool a = (bool)value;
            if (a) return Visibility.Collapsed;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Visibility a = (Visibility)value;
            if (Visibility.Collapsed.Equals(a)) return true;
            else return false;
        }
    }
}
