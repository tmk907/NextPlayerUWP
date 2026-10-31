using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;

namespace NextPlayerWinUI.Navigation
{
    public abstract class ViewModelBase : ObservableObject, INavigable
    {
        public virtual Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state) => Task.CompletedTask;

        public virtual Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending) => Task.CompletedTask;

        public virtual Task OnNavigatingFromAsync(NavigatingEventArgs args) => Task.CompletedTask;

        [JsonIgnore]
        public virtual INavigationService NavigationService { get; set; }

        [JsonIgnore]
        public virtual IDispatcherWrapper Dispatcher { get; set; }
    }
}
