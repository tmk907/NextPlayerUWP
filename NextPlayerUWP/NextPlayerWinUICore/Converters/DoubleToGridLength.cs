using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
{
    public class DoubleToGridLength : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double val = (double)value;
            GridLength gridLength = new GridLength(val);

            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }
    }
}
