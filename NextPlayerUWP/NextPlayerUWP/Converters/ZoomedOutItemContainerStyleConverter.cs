using NextPlayerUWP.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class ZoomedOutItemContainerStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return 92;
            //ComboBoxItemValue item = (ComboBoxItemValue)value;
            //if (item.Option == SortNames.Duration)
            //{
            //    return App.Current.Resources["ZoomedOutContainerStyleDuration"] as Style;
            //}
            //else if (item.Option == SortNames.Year || item.Option == SortNames.PlayCount || item.Option == SortNames.SongCount)
            //{
            //    return App.Current.Resources["ZoomedOutContainerStyleNumber"] as Style;
            //}
            //else if (item.Option == SortNames.LastAdded || item.Option == SortNames.LastPlayed)
            //{
            //    return App.Current.Resources["ZoomedOutContainerStyleDate"] as Style;
            //}
            //else
            //{
            //    return App.Current.Resources["ZoomedOutContainerStyleCharacter"] as Style;
            //}
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
