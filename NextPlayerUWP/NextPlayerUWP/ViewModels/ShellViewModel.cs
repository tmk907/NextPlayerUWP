using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class ShellViewModel : Template10.Mvvm.BindableBase
    {
        private bool isNowPlayingDesktopViewActive = true;
        public bool IsNowPlayingDesktopViewActive
        {
            get { return isNowPlayingDesktopViewActive; }
            set { Set(ref isNowPlayingDesktopViewActive, value); }
        }
    }
}
