using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.Controls
{
    public class AlternatingRowListView : ListView
    {
        public static readonly DependencyProperty OddRowBackgroundProperty = DependencyProperty.Register("OddRowBackground", typeof(Brush), typeof(AlternatingRowListView), null);
        public Brush OddRowBackground
        {
            get { return (Brush)GetValue(OddRowBackgroundProperty); }
            set { SetValue(OddRowBackgroundProperty, (Brush)value); }
        }

        public static readonly DependencyProperty EvenRowBackgroundProperty = DependencyProperty.Register("EvenRowBackground", typeof(Brush), typeof(AlternatingRowListView), null);
        public Brush EvenRowBackground
        {
            get { return (Brush)GetValue(EvenRowBackgroundProperty); }
            set { SetValue(EvenRowBackgroundProperty, (Brush)value); }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            
            var listViewItem = element as ListViewItem;
            
            if (listViewItem != null)
            {
                var index = IndexFromContainer(element);

                bool isEven = index % 2 == 0;

                if (this.IsGrouping)
                {
                    if (DataContext.GetType() == typeof(SongsViewModel))
                    {
                        var dt = DataContext as SongsViewModel;
                        var o = Items[index];
                        var i = dt.GroupedSongs.FirstOrDefault(g => g.Contains(o)).IndexOf(o);
                        isEven = i % 2 == 0;
                    }
                    else if (DataContext.GetType() == typeof(ArtistsViewModel))
                    {
                        var dt = DataContext as ArtistsViewModel;
                        var o = Items[index];
                        var i = dt.GroupedArtists.FirstOrDefault(g => g.Contains(o)).IndexOf(o);
                        isEven = i % 2 == 0;
                    }
                    else if (DataContext.GetType() == typeof(ArtistViewModel))
                    {
                        //error argsNullReferenceException
                        var dt = DataContext as ArtistViewModel;
                        var o = Items[index];
                        var i = dt.Albums.FirstOrDefault(g => g.Contains(o)).IndexOf(o);
                        isEven = i % 2 == 0;
                    }
                }

                if (isEven)
                {
                    listViewItem.Background = EvenRowBackground;
                }
                else
                {
                    listViewItem.Background = OddRowBackground;
                }
            }
        }
    }
}
