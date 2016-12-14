using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Controls
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
