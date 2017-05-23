using NextPlayerUWP.Common;
using System.Collections.ObjectModel;
using Template10.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace NextPlayerUWP.ViewModels
{
    public class ShellViewModel : Template10.Mvvm.BindableBase
    {
        public ShellViewModel()
        {
            App.MenuItemVisibilityChange += RefreshMenuButtons;
            var window = CoreApplication.GetCurrentView()?.CoreWindow;
            if (window != null)
            {
                window.SizeChanged += OnCoreWindowOnSizeChanged;
            }
            if (window.Bounds.Width >= normal)
            {
                IsRightPanelVisible = true;
                if (window.Bounds.Width > wide)
                {
                    RightPanelWidth = 300;
                }
                else
                {
                    RightPanelWidth = 250;
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
            set { Set(ref isRightPanelVisible, value); }
        }

        private int rightPanelWidth = 0;
        public int RightPanelWidth
        {
            get { return rightPanelWidth; }
            set { Set(ref rightPanelWidth, value); }
        }

        int compact = 0;
        int narrow = 500;
        int normal = 720;
        int wide = 1008;


        private void OnCoreWindowOnSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            if (args.Size.Width >= wide)
            {
                IsRightPanelVisible = true;
                RightPanelWidth = 300;
            }
            else if (args.Size.Width >= normal)
            {
                IsRightPanelVisible = true;
                RightPanelWidth = 250;
            }
            else if (args.Size.Width >= narrow)
            {
                IsRightPanelVisible = false;
                RightPanelWidth = 0;
            }
            else if (args.Size.Width >= compact)
            {
                IsRightPanelVisible = false;
                RightPanelWidth = 0;
            }
        }
    }
}
