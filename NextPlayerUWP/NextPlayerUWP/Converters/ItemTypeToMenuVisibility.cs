using NextPlayerUWPDataLayer.Enums;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class ItemTypeToMenuVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            MusicSource type = (MusicSource)value;
            string command = parameter as string;
            if (String.IsNullOrEmpty(command))
            {
                return Visibility.Visible;
            }
            else
            {
                if (type == MusicSource.LocalFile)
                {
                    return Visibility.Visible;
                }
                if (command == "addtoplaylist")
                {
                    if (type == MusicSource.RadioJamendo || type == MusicSource.Unknown || type == MusicSource.Radio)
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
                else if (command == "gotoartist" || command == "gotoalbum" || command == "showdetails" || command == "edittags")
                {
                    if (MusicSource.OnlineFile == type)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
