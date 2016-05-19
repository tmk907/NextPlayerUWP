using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Helpers
{
    public class ListViewItemTemplateSelector : DataTemplateSelector
    {
        //These are public properties that will be used in the Resources section of the XAML.
        public DataTemplate DarkItemTemplate { get; set; }
        public DataTemplate LightItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null)
            {
                ListViewItem lvItem = (ListViewItem)container;
                ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
                int index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);

                if (index % 2 == 0)
                {
                    return DarkItemTemplate;
                }
                else
                {
                    return LightItemTemplate;
                }
                
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
