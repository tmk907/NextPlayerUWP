using NextPlayerUWP.ViewModels;
using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class LanguageComboBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (LanguageItem)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (LanguageItem)value;
        }
    }
}
