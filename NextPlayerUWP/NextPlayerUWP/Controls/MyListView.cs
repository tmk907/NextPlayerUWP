using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Controls
{
    public class MyListView : ListView, IGetSelectedItems
    {
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
    }

    public class MyGridView : GridView, IGetSelectedItems
    {
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
    }
}
