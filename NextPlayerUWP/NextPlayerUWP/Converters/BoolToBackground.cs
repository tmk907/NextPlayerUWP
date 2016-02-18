﻿using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.Converters
{
    public class BoolToBackground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return new SolidColorBrush(Windows.UI.Colors.LightBlue);
            }
            else
            {
                return new SolidColorBrush(Windows.UI.Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
