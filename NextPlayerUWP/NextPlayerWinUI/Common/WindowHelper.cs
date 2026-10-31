using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

namespace NextPlayerWinUI.Common
{
    /// <summary>Access to the main window for WinRT APIs that used to find it through CoreWindow.</summary>
    public static class WindowHelper
    {
        public static IntPtr Handle => WindowNative.GetWindowHandle(App.Window);

        public static XamlRoot XamlRoot => App.Window.Content.XamlRoot;

        /// <summary>Associates a picker, secondary tile or similar WinRT object with the main window.</summary>
        public static T InitializeWithWindow<T>(T target) where T : class
        {
            WinRT.Interop.InitializeWithWindow.Initialize(target, Handle);
            return target;
        }
    }
}
