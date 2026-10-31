using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using System.Threading;
//using CommunityToolkit.WinUI.Animations;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerWinUI.Controls
{
    public sealed partial class PopupNotification : UserControl
    {
        public PopupNotification()
        {
            this.InitializeComponent();
            cts = new CancellationTokenSource();
        }

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Duration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(PopupNotification), new PropertyMetadata(TimeSpan.FromMilliseconds(1000)));


        public TimeSpan ShowAnimationDuration
        {
            get { return (TimeSpan)GetValue(ShowAnimationDurationProperty); }
            set { SetValue(ShowAnimationDurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowAnimationDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAnimationDurationProperty =
            DependencyProperty.Register("ShowAnimationDuration", typeof(TimeSpan), typeof(PopupNotification), new PropertyMetadata(TimeSpan.FromMilliseconds(700)));


        public TimeSpan HideAnimationDuration
        {
            get { return (TimeSpan)GetValue(HideAnimationDurationProperty); }
            set { SetValue(HideAnimationDurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HideAnimationDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideAnimationDurationProperty =
            DependencyProperty.Register("HideAnimationDuration", typeof(TimeSpan), typeof(PopupNotification), new PropertyMetadata(TimeSpan.FromMilliseconds(300)));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                NotificationText.Text = value;
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(PopupNotification), new PropertyMetadata(""));


        public string TextBottom
        {
            get { return (string)GetValue(TextBottomProperty); }
            set { SetValue(TextBottomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBottom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBottomProperty =
            DependencyProperty.Register("TextBottom", typeof(string), typeof(PopupNotification), new PropertyMetadata(""));


        public async void ShowNotification()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            try
            {
                await Start(cts.Token);
            }
            catch (OperationCanceledException ex)
            {
            }
        }

        public void ShowNotification(string messageTop, string messageBottom = null)
        {
            Text = messageTop;
            if (messageBottom == null)
            {
                TextBottom = "";
                BottomRow.Height = new GridLength(0);
            }
            else
            {
                TextBottom = messageBottom;
                BottomRow.Height = new GridLength(1, GridUnitType.Star);
            }
            ShowNotification();
        }

        CancellationTokenSource cts;

        private async Task Start(CancellationToken token)
        {
            await Show();
            token.ThrowIfCancellationRequested();
            await Task.Delay(Duration);
            token.ThrowIfCancellationRequested();
            await Hide();
        }

        private async Task Show()
        {
            NotificationPanel.Opacity = 0;
            NotificationPanel.RenderTransform = new TranslateTransform { X = SlideDistance };
            NotificationPanel.Visibility = Visibility.Visible;
            await NotificationPanel.AnimateAsync(ShowAnimationDuration, opacity: 1, offsetX: 0);
        }

        private async Task Hide()
        {
            await NotificationPanel.AnimateAsync(HideAnimationDuration, opacity: 0, offsetX: SlideDistance);
            NotificationPanel.Visibility = Visibility.Collapsed;
        }

        private const double SlideDistance = 100;
    }
}
