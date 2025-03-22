using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NextPlayerWinUI.Controls
{
    public class SizeNotifyPanel : ContentPresenter
    {
        public static DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(Size), typeof(SizeNotifyPanel), null);

        public Size Size
        {
            get { return (Size)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public SizeNotifyPanel()
        {
            SizeChanged += (s, e) => Size = e.NewSize;
        }
    }
}
