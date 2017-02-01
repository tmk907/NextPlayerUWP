using NextPlayerUWP.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class ShellViewModel : Template10.Mvvm.BindableBase
    {
        public ShellViewModel()
        {
            App.MenuItemVisibilityChange += RefreshMenuButtons;
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
            HamburgerMenuBuilder builder = new HamburgerMenuBuilder();
            PrimaryButtons.Clear();
            PrimaryButtons = builder.GetButtons();
        }
    }
}
