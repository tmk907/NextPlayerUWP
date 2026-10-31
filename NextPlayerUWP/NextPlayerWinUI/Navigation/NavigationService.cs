using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using NextPlayerWinUI.Common;

namespace NextPlayerWinUI.Navigation
{
    /// <summary>
    /// Wraps the shell's <see cref="Frame"/>, raising <see cref="INavigable"/> callbacks on the page's view model
    /// and keeping per-page state for the lifetime of the app.
    /// </summary>
    public sealed class NavigationService : INavigationService
    {
        private readonly Dictionary<(Type Page, int Depth), Dictionary<string, object>> pageStates = new();

        public NavigationService(Frame frame)
        {
            Frame = frame;
            Frame.Navigating += OnFrameNavigating;
            Frame.Navigated += OnFrameNavigated;
        }

        public Frame Frame { get; }
        public bool CanGoBack => Frame.CanGoBack;
        public Type CurrentPageType => Frame.CurrentSourcePageType;
        public object CurrentPageParam { get; private set; }

        public Task<bool> NavigateAsync(Type page, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            ArgumentNullException.ThrowIfNull(page);

            if (page == CurrentPageType && Equals(parameter, CurrentPageParam))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(Frame.Navigate(page, parameter, infoOverride));
        }

        public Task<bool> NavigateAsync(AppPages.Pages page, object parameter = null, NavigationTransitionInfo infoOverride = null)
            => NavigateAsync(App.AppPages.PagesToTypes[page], parameter, infoOverride);

        public void Navigate(Type page, object parameter = null, NavigationTransitionInfo infoOverride = null)
            => _ = NavigateAsync(page, parameter, infoOverride);

        public void Navigate(AppPages.Pages page, object parameter = null, NavigationTransitionInfo infoOverride = null)
            => _ = NavigateAsync(page, parameter, infoOverride);

        public void GoBack(NavigationTransitionInfo infoOverride = null)
        {
            if (!Frame.CanGoBack)
            {
                return;
            }

            if (infoOverride == null)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.GoBack(infoOverride);
            }
        }

        public void ClearHistory() => Frame.BackStack.Clear();

        private async void OnFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Frame.Content is not Page page || page.DataContext is not INavigable viewModel)
            {
                return;
            }

            Prepare(viewModel);
            var state = GetPageState(page.GetType(), Frame.BackStackDepth);
            await viewModel.OnNavigatingFromAsync(new NavigatingEventArgs
            {
                NavigationMode = e.NavigationMode,
                PageType = page.GetType(),
                Parameter = CurrentPageParam,
            });
            await viewModel.OnNavigatedFromAsync(state, false);
        }

        private async void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            CurrentPageParam = e.Parameter;

            if (e.Content is not Page page)
            {
                return;
            }

            var state = GetPageState(page.GetType(), Frame.BackStackDepth);
            if (e.NavigationMode == NavigationMode.New)
            {
                state.Clear();
            }

            if (page.DataContext is INavigable viewModel)
            {
                Prepare(viewModel);
                await viewModel.OnNavigatedToAsync(e.Parameter, e.NavigationMode, state);

                // Pages use OneTime x:Bind on values the view model only sets above, so refresh them.
                UpdateBindings(page);
            }
        }

        private void Prepare(INavigable viewModel)
        {
            viewModel.NavigationService = this;
            viewModel.Dispatcher = App.Dispatcher;
        }

        private Dictionary<string, object> GetPageState(Type page, int depth)
        {
            if (!pageStates.TryGetValue((page, depth), out var state))
            {
                state = new Dictionary<string, object>();
                pageStates[(page, depth)] = state;
            }

            return state;
        }

        private static void UpdateBindings(Page page)
        {
            var bindings = page.GetType().GetTypeInfo().GetDeclaredField("Bindings")?.GetValue(page);
            bindings?.GetType().GetRuntimeMethod("Update", Type.EmptyTypes)?.Invoke(bindings, null);
        }
    }
}
