using NextPlayerUWP.Common;
using System;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class BoolToSubfoldersDescription : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool a = (bool)value;
            TranslationHelper tr = new TranslationHelper();
            if (a) return tr.GetTranslation(TranslationHelper.IncludeSubfolders);
            else return tr.GetTranslation(TranslationHelper.DontIncludeSubFolders);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
