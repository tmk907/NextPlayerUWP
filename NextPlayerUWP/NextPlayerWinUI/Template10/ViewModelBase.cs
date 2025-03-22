using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using NextPlayerWinUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Services.NavigationService;

namespace Template10.Mvvm
{
    public abstract class ViewModelBase : ObservableObject, INavigable
    {
        public virtual Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual INavigationService NavigationService { get; set; }

        public virtual IDispatcherWrapper Dispatcher { get; set; }

        //public virtual IStateItems SessionState { get; set; }
    }
}
