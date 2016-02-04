using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace NextPlayerUWP.Converters
{
    public class AlbumImagePath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //AlbumItem album = (AlbumItem)value;
            string path = value as string;
            if (path == null || path == "") path = "ms-appx:///Albums/DefaultAlbumCover.png";
            //if (album.ImagePath == "")
            //{
            //    path = ImagesManager.GetAlbumCoverPath(album).Result;
            //    album.ImagePath = path;
            //    DatabaseManager.Current.UpdateAlbumItem(album).RunSynchronously();
            //}
            Uri uri = new Uri(path);
            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
