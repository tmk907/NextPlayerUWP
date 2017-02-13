using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWP.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
using Template10.Services.NavigationService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView2 : Page
    {
        private SettingsMenuItem _lastSelectedItem;

        public SettingsView2()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var items = MasterListView.ItemsSource as List<SettingsMenuItem>;

            if (items == null)
            {
                ViewModelLocator vml = new ViewModelLocator();
                items = SettingsMenuItem.GetMainItems(vml.SettingsVMService);

                MasterListView.ItemsSource = items;
            }

            if (e.Parameter != null)
            {
                // Parameter is item ID
                var id = (string)e.Parameter;
                _lastSelectedItem = items.Where((item) => item.TypeName == id).FirstOrDefault();
            }

            UpdateForVisualState(AdaptiveStates.CurrentState);

            // Don't play a content transition for first item load.
            // Sometimes, this content will be animated as part of the page transition.
            DisableContentTransitions();
        }

        private void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (SettingsMenuItem)e.ClickedItem;
            _lastSelectedItem = clickedItem;
            DetailContentPresenter.Content = clickedItem.ViewModel;
            if (AdaptiveStates.CurrentState == NarrowState)
            {
                // Use "drill in" transition for navigating from master list to detail view
                var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
                nav.Navigate(typeof(SettingsDetailsView), clickedItem.TypeName, new DrillInNavigationTransitionInfo());
                //Frame.Navigate(typeof(SettingsDetailsView), clickedItem.TypeName, new DrillInNavigationTransitionInfo());
            }
            else
            {
                //Play a refresh animation when the user switches detail items.
                EnableContentTransitions();
            }
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            // Assure we are displaying the correct item. This is necessary in certain adaptive cases.
            MasterListView.SelectedItem = _lastSelectedItem;
        }

        private void EnableContentTransitions()
        {
            DetailContentPresenter.ContentTransitions.Clear();
            DetailContentPresenter.ContentTransitions.Add(new EntranceThemeTransition());
        }

        private void DisableContentTransitions()
        {
            if (DetailContentPresenter != null)
            {
                DetailContentPresenter.ContentTransitions.Clear();
            }
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;

            if (isNarrow && oldState == DefaultState && _lastSelectedItem != null)
            {
                // Resize down to the detail item. Don't play a transition.
                var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
                nav.Navigate(typeof(SettingsDetailsView), _lastSelectedItem.TypeName, new SuppressNavigationTransitionInfo());
                //Frame.Navigate(typeof(SettingsDetailsView), _lastSelectedItem.TypeName, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(MasterListView, isNarrow);
            if (DetailContentPresenter != null)
            {
                EntranceNavigationTransitionInfo.SetIsTargetElement(DetailContentPresenter, !isNarrow);
            }
        }
    }

    public class SettingsMenuItem
    {
        public Symbol Icon { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public ISettingsViewModel ViewModel { get; set; }

        public static List<SettingsMenuItem> GetMainItems(SettingsVMService service)
        {
            var items = new List<SettingsMenuItem>();
            items.Add(new SettingsMenuItem() { Icon = Symbol.Library, Name = "Library", ViewModel = service.GetViewModelByName(nameof(SettingsLibraryViewModel)), TypeName = nameof(SettingsLibraryViewModel) });
            items.Add(new SettingsMenuItem() { Icon = Symbol.Edit, Name = "Personalization", ViewModel = service.GetViewModelByName(nameof(SettingsPersonalizationViewModel)), TypeName = nameof(SettingsPersonalizationViewModel) });
            items.Add(new SettingsMenuItem() { Icon = Symbol.Clock, Name = "Tools", ViewModel = service.GetViewModelByName(nameof(SettingsToolsViewModel)), TypeName = nameof(SettingsToolsViewModel) });
            items.Add(new SettingsMenuItem() { Icon = Symbol.People, Name = "Accounts", ViewModel = service.GetViewModelByName(nameof(SettingsAccountsViewModel)), TypeName = nameof(SettingsAccountsViewModel) });
            items.Add(new SettingsMenuItem() { Icon = Symbol.Next, Name = "About", ViewModel = service.GetViewModelByName(nameof(SettingsAboutViewModel)), TypeName = nameof(SettingsAboutViewModel) });
            return items;
        }

        public static List<SettingsMenuItem> GetOptionsItems()
        {
            var items = new List<SettingsMenuItem>();
            return items;
        }
    }
}
