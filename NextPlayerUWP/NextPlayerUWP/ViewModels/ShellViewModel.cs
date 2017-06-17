using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using System.Collections.ObjectModel;
using Template10.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace NextPlayerUWP.ViewModels
{
    public class ShellViewModel : Template10.Mvvm.BindableBase
    {
        public ShellViewModel()
        {
            App.MenuItemVisibilityChange += RefreshMenuButtons;
            var window = CoreApplication.GetCurrentView()?.CoreWindow;
            rightPanelCompact = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthCompact);
            rightPanelNarrow = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthNarrow);
            rightPanelNormal = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthNormal);
            rightPanelWide = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthWide);
            if (window != null)
            {
                window.SizeChanged += OnCoreWindowOnSizeChanged;
            }
            if (window.Bounds.Width >= normal)
            {
                IsRightPanelVisible = true;
                if (window.Bounds.Width > wide)
                {
                    RightPanelWidth = rightPanelWide;
                    RightPanelMinWidth = 300.0;
                    prevWindowState = wide;
                }
                else
                {
                    RightPanelWidth = rightPanelNormal;
                    RightPanelMinWidth = 250.0;
                    prevWindowState = normal;
                }
            }
        }

        private bool isNowPlayingDesktopViewActive = true;
        public bool IsNowPlayingDesktopViewActive
        {
            get { return isNowPlayingDesktopViewActive; }
            set { Set(ref isNowPlayingDesktopViewActive, value); }
        }
       
        private ObservableCollection<HamburgerButtonInfo> primaryButtons = new ObservableCollection<HamburgerButtonInfo>();
        public ObservableCollection<HamburgerButtonInfo> PrimaryButtons
        {
            get { return primaryButtons; }
            set { Set(ref primaryButtons, value); }
        }

        public void RefreshMenuButtons()
        {
            System.Diagnostics.Debug.WriteLine("Refreshmenubuttons");
            HamburgerMenuBuilder builder = new HamburgerMenuBuilder();
            PrimaryButtons.Clear();
            PrimaryButtons = builder.GetButtons();
        }

        private bool isRightPanelVisible = false;
        public bool IsRightPanelVisible
        {
            get { return isRightPanelVisible; }
            set
            {
                bool changed = (value != isRightPanelVisible);
                Set(ref isRightPanelVisible, value);
                if (changed) Messages.Hub.MessageHub.Instance.Publish(new Messages.RightPanelVisibilityChange() { Visible = value });
            }
        }

        private double rightPanelWidth = 0;
        public double RightPanelWidth
        {
            get { return rightPanelWidth; }
            set { Set(ref rightPanelWidth, value); }
        }

        private double rightPanelMinWidth = 0;
        public double RightPanelMinWidth
        {
            get { return rightPanelMinWidth; }
            set { Set(ref rightPanelMinWidth, value); }
        }

        const double compact = 0;
        const double narrow = 500;
        const double normal = 720;
        const double wide = 1008;

        double rightPanelCompact = 0;
        double rightPanelNarrow = 0;
        double rightPanelNormal = 250;
        double rightPanelWide = 300;

        private double prevWindowState = compact;

        private void OnCoreWindowOnSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            if (args.Size.Width >= wide)
            {
                if (prevWindowState != wide)
                {
                    System.Diagnostics.Debug.WriteLine("WindowSizeChanged wide");
                    prevWindowState = wide;
                    IsRightPanelVisible = true;
                    RightPanelWidth = rightPanelWide;
                    RightPanelMinWidth = 250.0;
                    //Messages.Hub.MessageHub.Instance.Publish(new Messages.WindowSizeBreakpoint() { Width = wide });
                }
            }
            else if (args.Size.Width >= normal)
            {
                if (prevWindowState != normal)
                {
                    System.Diagnostics.Debug.WriteLine("WindowSizeChanged normal");
                    prevWindowState = normal;
                    IsRightPanelVisible = true;
                    RightPanelWidth = rightPanelNormal;
                    RightPanelMinWidth = 250.0;
                    //Messages.Hub.MessageHub.Instance.Publish(new Messages.WindowSizeBreakpoint() { Width = normal });
                }
            }
            else if (args.Size.Width >= narrow)
            {
                if (prevWindowState != narrow)
                {
                    System.Diagnostics.Debug.WriteLine("WindowSizeChanged narrow");
                    prevWindowState = narrow;
                    IsRightPanelVisible = false;
                    RightPanelWidth = rightPanelNarrow;
                    RightPanelMinWidth = 0.0;
                    //Messages.Hub.MessageHub.Instance.Publish(new Messages.WindowSizeBreakpoint() { Width = narrow });
                }
            }
            else if (args.Size.Width >= compact)
            {
                if (prevWindowState != compact)
                {
                    System.Diagnostics.Debug.WriteLine("WindowSizeChanged compact");
                    prevWindowState = compact;
                    IsRightPanelVisible = false;
                    RightPanelWidth = rightPanelCompact;
                    RightPanelMinWidth = 0.0;
                    //Messages.Hub.MessageHub.Instance.Publish(new Messages.WindowSizeBreakpoint() { Width = compact });
                }
            }
        }

        private readonly CoreCursor normalCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        private readonly CoreCursor hoverCursor = new CoreCursor(CoreCursorType.SizeWestEast, 1);
        private bool resizing = false;

        public void GridSplitterManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            resizing = true;
            Window.Current.CoreWindow.PointerCursor = hoverCursor;
        }

        public void GridSplitterManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (rightPanelWidth - e.Delta.Translation.X < rightPanelMinWidth)
            {
                RightPanelWidth = rightPanelMinWidth;
            }
            else
            {
                RightPanelWidth -= e.Delta.Translation.X;
            }
        }

        public void GridSplitterManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ManipulationCompleted {0}", rightPanelWidth);
            if (prevWindowState == wide)
            {
                rightPanelWide = rightPanelWidth;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.RightPanelWidthWide, rightPanelWidth);
            }
            else if (prevWindowState == normal)
            {
                rightPanelNormal = rightPanelWidth;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.RightPanelWidthNormal, rightPanelWidth);
            }
            else if (prevWindowState == narrow)
            {
                rightPanelWide = rightPanelWidth;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.RightPanelWidthNarrow, rightPanelWidth);
            }
            else if (prevWindowState == compact)
            {
                rightPanelCompact = rightPanelWidth;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.RightPanelWidthCompact, rightPanelWidth);
            }
            resizing = false;
            Window.Current.CoreWindow.PointerCursor = normalCursor;
        }

        public void GridSplitterPointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!resizing)
            {
                Window.Current.CoreWindow.PointerCursor = hoverCursor;
            }
        }

        public void GridSplitterPointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!resizing)
            {
                Window.Current.CoreWindow.PointerCursor = normalCursor;
            }
        }
    }
}
