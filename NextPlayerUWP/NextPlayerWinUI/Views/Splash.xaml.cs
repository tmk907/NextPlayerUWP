using Windows.ApplicationModel.Activation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerWinUI.Views
{
    public sealed partial class Splash : UserControl
    {
        public Splash(SplashScreen splashScreen)
        {
            this.InitializeComponent();
            //Window.Current.SizeChanged += (s, e) => Resize(splashScreen);
            //Resize(splashScreen);
        }

        //private void Resize(SplashScreen splashScreen)
        //{
        //    if (splashScreen.ImageLocation.Top == 0)
        //    {
        //        splashImage.Visibility = Visibility.Collapsed;
        //        return;
        //    }
        //    else
        //    {
        //        rootCanvas.Background = null;
        //        splashImage.Visibility = Visibility.Visible;
        //    }
        //    splashImage.Height = splashScreen.ImageLocation.Height;
        //    splashImage.Width = splashScreen.ImageLocation.Width;
        //    splashImage.SetValue(Canvas.TopProperty, splashScreen.ImageLocation.Top);
        //    splashImage.SetValue(Canvas.LeftProperty, splashScreen.ImageLocation.Left);
        //    ProgressTransform.TranslateY = splashImage.Height / 2;
        //}
    }
}
