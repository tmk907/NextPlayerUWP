using NextPlayerUWPDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWPDataLayer.Helpers
{
    public enum RepeatEnum
    {
        NoRepeat,
        RepeatOnce,
        RepeatPlaylist
    }

    public class Repeat
    {
        public static readonly string NoRepeat = "\uE1cd";//\uE17e
        public static readonly string RepeatOnce = "\uE1cc";
        public static readonly string RepeatPlaylist = "\uE1cd";

        private static RepeatEnum Next(RepeatEnum e)
        {
            switch (e)
            {
                case RepeatEnum.NoRepeat:
                    return RepeatEnum.RepeatOnce;
                case RepeatEnum.RepeatOnce:
                    return RepeatEnum.RepeatPlaylist;
                case RepeatEnum.RepeatPlaylist:
                    return RepeatEnum.NoRepeat;
                default:
                    return RepeatEnum.NoRepeat;
            }
        }

        public static RepeatEnum Change()
        {
            RepeatEnum newEnum = Next(CurrentState());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Repeat, newEnum.ToString());
            return newEnum;
        }

        public static RepeatEnum CurrentState()
        {
            RepeatEnum repeat;
            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Repeat);
            if (o != null)
            {
               repeat = (RepeatEnum)Enum.Parse(typeof(RepeatEnum), (string) o, true);
            }
            else
            {
                repeat = RepeatEnum.NoRepeat;
            }
            return repeat;
        }

        public static SolidColorBrush GetColor(RepeatEnum repeat, bool adjustToTheme = false)
        {
            switch (repeat)
            {
                case RepeatEnum.NoRepeat:
                    if (adjustToTheme)
                    {
                        bool isLight = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);
                        if (isLight)
                        {
                            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 204, 204, 204));
                        }
                        else
                        {
                            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 119, 119, 119));
                        }
                    }
                    else return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 204, 204, 204));
                case RepeatEnum.RepeatOnce:
                    if (adjustToTheme)
                    {
                        bool isLight = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);
                        if (isLight)
                        {
                            return new SolidColorBrush(Windows.UI.Colors.Black);
                        }
                    }
                    return new SolidColorBrush(Windows.UI.Colors.White);
                case RepeatEnum.RepeatPlaylist:
                    if (adjustToTheme)
                    {
                        bool isLight = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);
                        if (isLight)
                        {
                            return new SolidColorBrush(Windows.UI.Colors.Black);
                        }
                    }
                    return new SolidColorBrush(Windows.UI.Colors.White);
                default:
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 204, 204, 204));
            }
        }

        public static string GetContent(RepeatEnum repeat)
        {
            switch (repeat)
            {
                case RepeatEnum.NoRepeat:
                    return "\uE8EE";
                    //return Symbol.RepeatAll;
                case RepeatEnum.RepeatOnce:
                    return "\uE8ED";
                    //return Symbol.RepeatOne;
                case RepeatEnum.RepeatPlaylist:
                    return "\uE8EE";
                    //return Symbol.RepeatAll;
                default:
                    return "\uE8EE";
                    //return Symbol.RepeatAll;
            }
        }
    }
}
