using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Template10.Services.NavigationService
{
    public class NavigatingEventArgs : NavigatedEventArgs
    {
        public NavigatingEventArgs() : base()
        {
           
        }

        public NavigatingEventArgs(NavigatingCancelEventArgs e, Page page, object parameter) : this()
        {
            NavigationMode = e.NavigationMode;
            PageType = e.SourcePageType;
            Page = page;
            Parameter = parameter;
        }

        public bool Cancel { get; set; } = false;
        public bool Suspending { get; set; } = false;
    }
}
