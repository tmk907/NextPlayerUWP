﻿using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class DateTimeToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime date = (DateTime)value;
            if (DateTime.MinValue.Equals(date)) return "----";
            return date.ToString("d");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            DateTime date;
            DateTime.TryParse((string)value,out date);
            return date;
        }
    }
}
