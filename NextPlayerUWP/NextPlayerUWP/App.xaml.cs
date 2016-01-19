using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Template10.Common.BootStrapper
    {
        public App()
        {
            InitializeComponent();
        }

        public enum Pages
        {
            Albums,
            Album,
            Artists,
            Songs,
            Settings
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            //insights
            var keys = PageKeys<Pages>();
            //keys.Add(Pages.MainPage, typeof(MainPage));

            return base.OnInitializeAsync(args);
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            AdditionalKinds cause = DetermineStartCause(args);
            //if (cause == AdditionalKinds.SecondaryTile)
            //{
            //    LaunchActivatedEventArgs eventArgs = args as LaunchActivatedEventArgs;
            //    NavigationService.Navigate(typeof(DetailPage), eventArgs.Arguments);
            //}
            //else
            //{
            //    NavigationService.Navigate(typeof(MainPage));
            //}
            NavigationService.Navigate(typeof(MainPage));
            return Task.FromResult<object>(null);
        }
    }
}
