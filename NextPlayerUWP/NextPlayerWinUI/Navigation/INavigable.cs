using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;

namespace NextPlayerWinUI.Navigation
{
    /// <summary>Implemented by view models that want page lifecycle callbacks from <see cref="NavigationService"/>.</summary>
    public interface INavigable
    {
        Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state);
        Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending);
        Task OnNavigatingFromAsync(NavigatingEventArgs args);
        INavigationService NavigationService { get; set; }
        IDispatcherWrapper Dispatcher { get; set; }
    }

    public class NavigatingEventArgs : EventArgs
    {
        public NavigationMode NavigationMode { get; init; }
        public Type PageType { get; init; }
        public object Parameter { get; init; }
        public bool Suspending { get; init; }
        public bool Cancel { get; set; }
    }
}
