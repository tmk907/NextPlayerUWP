using NextPlayerUWPDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class Shuffle
    {
        public static readonly string ShuffleButtonContent = "&#xE17e;&#xE14b;";

        public static bool Change()
        {
            bool s = CurrentState();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Shuffle, !s);
            return !s;
        }

        public static bool CurrentState()
        {
            bool b;
            object s = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Shuffle);
            if (s != null)
            {
                b = (bool)s;
            }
            else
            {
                b = false;
            }
            return b;
        }

        public static SolidColorBrush GetColor(bool shuffle)
        {
            if (shuffle)
            {
                if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                {
                    return new SolidColorBrush(Windows.UI.Colors.White);
                }
                else return new SolidColorBrush(Windows.UI.Colors.Black);
            }
            else
            {
                return new SolidColorBrush(Windows.UI.Colors.Gray);
            }
        }
    }
}
