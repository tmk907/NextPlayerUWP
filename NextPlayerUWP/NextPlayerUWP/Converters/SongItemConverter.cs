using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class SongItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (ObservableCollection<SongItem>)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (ObservableCollection<SongItem>)value;
        }
    }
}
