using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace NextPlayerWinUIDataLayer.Helpers
{
    public class Shuffle
    {
        public static readonly string ShuffleButtonContent = "&#xE17e;&#xE14b;";

        public static bool Change()
        {
            bool s = CurrentState();
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.Shuffle, !s);
            return !s;
        }

        public static bool CurrentState()
        {
            bool b;
            object s = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.Shuffle);
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

        public static SolidColorBrush GetColor(bool shuffle, bool adjustToTheme = false)
        {
            if (shuffle)
            {
                if (adjustToTheme)
                {
                    bool isLight = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppTheme);
                    if (isLight)
                    {
                        return new SolidColorBrush(Colors.Black);
                    }
                }
                return new SolidColorBrush(Colors.White);
            }
            else
            {
                if (adjustToTheme)
                {
                    bool isLight = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppTheme);
                    if (isLight)
                    {
                        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 204, 204, 204));
                    }
                    else
                    {
                        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 119, 119, 119));
                    }
                }
                else return new SolidColorBrush(Windows.UI.Color.FromArgb(255,204,204,204));
            }
        }
    }
}
