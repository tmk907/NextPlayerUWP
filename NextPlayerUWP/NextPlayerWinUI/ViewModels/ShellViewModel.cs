using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Helpers;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NextPlayerWinUI.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        public ShellViewModel()
        {
            App.MenuItemVisibilityChange += RefreshMenuButtons;
            var window = App.Window;
            rightPanelCompact = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthCompact);
            rightPanelNarrow = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthNarrow);
            rightPanelNormal = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthNormal);
            rightPanelWide = ApplicationSettingsHelper.ReadSettingsValue<double>(SettingsKeys.RightPanelWidthWide);
            if (window != null)
            {
                window.SizeChanged += OnWindowSizeChanged;
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

        private void OnWindowSizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
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

        private bool isNowPlayingDesktopViewActive = true;
        public bool IsNowPlayingDesktopViewActive
        {
            get { return isNowPlayingDesktopViewActive; }
            set { SetProperty(ref isNowPlayingDesktopViewActive, value); }
        }
       
        private ObservableCollection<NavigationViewItem> primaryButtons = new ObservableCollection<NavigationViewItem>();
        public ObservableCollection<NavigationViewItem> PrimaryButtons
        {
            get { return primaryButtons; }
            set { SetProperty(ref primaryButtons, value); }
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
                SetProperty(ref isRightPanelVisible, value);
                if (changed) Messages.Hub.MessageHub.Instance.Publish(new Messages.RightPanelVisibilityChange() { Visible = value });
            }
        }

        private double rightPanelWidth = 0;
        public double RightPanelWidth
        {
            get { return rightPanelWidth; }
            set { SetProperty(ref rightPanelWidth, value); }
        }

        private double rightPanelMinWidth = 0;
        public double RightPanelMinWidth
        {
            get { return rightPanelMinWidth; }
            set { SetProperty(ref rightPanelMinWidth, value); }
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

        public void GridSplitterManipulationStarted(object sender, Microsoft.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
        }

        public void GridSplitterManipulationDelta(object sender, Microsoft.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
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

        public void GridSplitterManipulationCompleted(object sender, Microsoft.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
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
                rightPanelNarrow = rightPanelWidth;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.RightPanelWidthNarrow, rightPanelWidth);
            }
            else if (prevWindowState == compact)
            {
                rightPanelCompact = rightPanelWidth;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.RightPanelWidthCompact, rightPanelWidth);
            }
        }
    }
}
