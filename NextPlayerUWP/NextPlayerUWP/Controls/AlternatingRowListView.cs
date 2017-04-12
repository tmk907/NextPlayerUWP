using NextPlayerUWP.ViewModels;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.Controls
{
    public class AlternatingRowListView : ListView, IGetSelectedItems
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

        public List<T> GetSelectedItems<T>()
        {
            List<T> list = new List<T>();
            if (SelectedItems != null)
            {
                foreach (var item in SelectedItems)
                {
                    list.Add((T)item);
                }
            }
            return list;
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
                    var dt = DataContext as IGroupedItemsList;
                    if (dt != null)
                    {
                        var o = Items[index];
                        var i = dt.GetIndexFromGroup(o);
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
