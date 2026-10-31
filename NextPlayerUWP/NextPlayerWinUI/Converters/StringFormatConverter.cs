using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public string Format { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var format = (parameter as string) ?? Format;
            if (format == null)
            {
                return value;
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                return string.Format(format, value);
            }

            try
            {
                return string.Format(new CultureInfo(language), format, value);
            }
            catch (CultureNotFoundException)
            {
                return string.Format(format, value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
