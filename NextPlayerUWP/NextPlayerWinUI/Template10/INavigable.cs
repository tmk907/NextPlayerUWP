using Microsoft.UI.Xaml.Navigation;
using NextPlayerWinUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Template10.Services.NavigationService
{
    public interface INavigable
    {
        Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state);
        Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending);
        Task OnNavigatingFromAsync(NavigatingEventArgs args);
        INavigationService NavigationService { get; set; }
        IDispatcherWrapper Dispatcher { get; set; }
        //IStateItems SessionState { get; set; }
    }
}
