using Microsoft.UI.Xaml;
using NextPlayerWinUIDataLayer.Helpers;
using System;
using System.Runtime.InteropServices;

namespace NextPlayerWinUI.Common
{
    public class ThemeHelper
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

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

        /// <summary>The system title bar follows this attribute on both Windows 10 and Windows 11.</summary>
        public static void ApplyThemeToTitleBar(ElementTheme theme)
        {
            if (App.Window == null)
            {
                return;
            }

            int useDarkMode = theme == ElementTheme.Dark ? 1 : 0;
            DwmSetWindowAttribute(WindowHelper.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }
    }
}
