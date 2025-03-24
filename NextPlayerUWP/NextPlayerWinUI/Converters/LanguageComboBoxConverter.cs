using NextPlayerWinUI.ViewModels.Settings;
using System;
using Microsoft.UI.Xaml.Data;

namespace NextPlayerWinUI.Converters
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
