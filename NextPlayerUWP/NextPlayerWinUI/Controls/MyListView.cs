using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace NextPlayerWinUI.Controls
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
