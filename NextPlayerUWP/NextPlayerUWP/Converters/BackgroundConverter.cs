using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.Converters
{
    public class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int index = (int)value;
            //int index = ((MusicItem)value).Index;
            if ((int)index % 2 == 0)
            {
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xf0, 0xf0, 0xf0));
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xfa, 0xfa, 0xfa));
                //return new SolidColorBrush(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
