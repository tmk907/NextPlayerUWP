using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using NextPlayerWinUI.Common;

namespace NextPlayerWinUI.Navigation
{
    public interface INavigationService
    {
        Frame Frame { get; }
        bool CanGoBack { get; }
        Type CurrentPageType { get; }
        object CurrentPageParam { get; }

        void Navigate(Type page, object parameter = null, NavigationTransitionInfo infoOverride = null);
        void Navigate(AppPages.Pages page, object parameter = null, NavigationTransitionInfo infoOverride = null);
        Task<bool> NavigateAsync(Type page, object parameter = null, NavigationTransitionInfo infoOverride = null);
        Task<bool> NavigateAsync(AppPages.Pages page, object parameter = null, NavigationTransitionInfo infoOverride = null);

        void GoBack(NavigationTransitionInfo infoOverride = null);
        void ClearHistory();
    }
}
