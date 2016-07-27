using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace NextPlayerUWP.Common
{
    public class ThemeHelper
    {
        public static void ApplyThemeToStatusBar(bool isLightTheme)
        {
            if (isLightTheme) ApplyThemeToStatusBar(ElementTheme.Light);
            else ApplyThemeToStatusBar(ElementTheme.Dark);
        }

        public static void ApplyThemeToStatusBar(ElementTheme theme)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                switch (theme)
                {
                    case ElementTheme.Dark:
                        statusBar.BackgroundColor = Colors.Black;
                        statusBar.BackgroundOpacity = 1;
                        statusBar.ForegroundColor = Colors.White;
                        break;
                    case ElementTheme.Light:
                        statusBar.BackgroundColor = Colors.White;
                        statusBar.BackgroundOpacity = 1;
                        statusBar.ForegroundColor = Colors.Black;
                        break;
                    default:
                        statusBar.BackgroundColor = Colors.Black;
                        statusBar.BackgroundOpacity = 1;
                        statusBar.ForegroundColor = Colors.White;
                        break;
                }
            }
        }

        public static void ApplyThemeToTitleBar(bool isLightTheme)
        {
            if (isLightTheme) ApplyThemeToTitleBar(ElementTheme.Light);
            else ApplyThemeToTitleBar(ElementTheme.Dark);
        }

        public static void ApplyThemeToTitleBar(ElementTheme theme)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    switch (theme)
                    {
                        case ElementTheme.Dark:
                            titleBar.BackgroundColor = Colors.Black;
                            titleBar.ButtonBackgroundColor = Colors.Black;
                            titleBar.ButtonForegroundColor = Colors.White;
                            titleBar.ForegroundColor = Colors.White;
                            break;
                        case ElementTheme.Light:
                            titleBar.BackgroundColor = Colors.White;
                            titleBar.ButtonBackgroundColor = Colors.White;
                            titleBar.ButtonForegroundColor = Colors.Black;
                            titleBar.ForegroundColor = Colors.Black;
                            break;
                        default:
                            titleBar.BackgroundColor = Colors.Black;
                            titleBar.ButtonBackgroundColor = Colors.Black;
                            titleBar.ButtonForegroundColor = Colors.White;
                            titleBar.ForegroundColor = Colors.White;
                            break;
                    }
                }
            }
        }
    }
}
