using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using NextPlayerWinUIDataLayer.Helpers;

namespace NextPlayerWinUI.Common
{
    public class ThemeHelper
    {
        public static bool IsLightTheme => (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppTheme);

        public static void ApplyAppTheme(bool isLightTheme)
        {
            var theme = isLightTheme ? ElementTheme.Light : ElementTheme.Dark;
            if (App.NavigationService != null)
            {
                App.NavigationService.Frame.RequestedTheme = theme;
            }
            ApplyThemeToTitleBar(theme);
        }

        public static void ApplyThemeToTitleBar(bool isLightTheme)
            => ApplyThemeToTitleBar(isLightTheme ? ElementTheme.Light : ElementTheme.Dark);

        public static void ApplyThemeToTitleBar(ElementTheme theme)
        {
            var titleBar = App.Window?.AppWindow.TitleBar;
            if (titleBar != null)
            {
                titleBar.PreferredTheme = theme == ElementTheme.Dark ? TitleBarTheme.Dark : TitleBarTheme.Light;
            }
        }
    }
}
