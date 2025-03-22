using NextPlayerWinUIDataLayer.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NextPlayerWinUI.Controls
{
    public class FolderDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FileTemplate { get; set; }
        public DataTemplate FolderTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is FolderItem)
                return FolderTemplate;
            if (item is SongItem)
                return FileTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
