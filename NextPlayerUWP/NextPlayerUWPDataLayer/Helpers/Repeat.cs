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

        public static SolidColorBrush GetColor(RepeatEnum repeat)
        {
            switch (repeat)
            {
                case RepeatEnum.NoRepeat:
                    return new SolidColorBrush(Windows.UI.Colors.Gray);
                case RepeatEnum.RepeatOnce:
                    if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        return new SolidColorBrush(Windows.UI.Colors.White);
                    }
                    else return new SolidColorBrush(Windows.UI.Colors.Black);
                case RepeatEnum.RepeatPlaylist:
                    if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        return new SolidColorBrush(Windows.UI.Colors.White);
                    }
                    else return new SolidColorBrush(Windows.UI.Colors.Black);
                default:
                    return new SolidColorBrush(Windows.UI.Colors.Gray);
            }
        }

        public static Symbol GetContent(RepeatEnum repeat)
        {
            switch (repeat)
            {
                case RepeatEnum.NoRepeat:
                    return Symbol.RepeatAll;
                case RepeatEnum.RepeatOnce:
                    return Symbol.RepeatOne;
                case RepeatEnum.RepeatPlaylist:
                    return Symbol.RepeatAll;
                default:
                    return Symbol.RepeatAll;
            }
        }
    }
}
