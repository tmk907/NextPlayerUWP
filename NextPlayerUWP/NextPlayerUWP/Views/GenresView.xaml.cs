﻿using GalaSoft.MvvmLight.Messaging;
using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerUWP.Controls;
using NextPlayerUWP.Messages;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GenresView : Page
    {
        public GenresViewModel ViewModel;
        private ButtonsForMultipleSelection selectionButtons;

        public GenresView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (GenresViewModel)DataContext;
            selectionButtons = new ButtonsForMultipleSelection();
        }

        //~GenresView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded(GenresListView);
            Messenger.Default.Register<NotificationMessage<EnableSearching>>(this, (message) =>
            {
                SearchBox.Focus(FocusState.Programmatic);
            });
            selectionButtons.OnLoaded(ViewModel, PageHeader, GenresListView);
        }

        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            selectionButtons.OnUnloaded();
            Messenger.Default.Unregister(this);
            ViewModel.OnUnloaded();
            //ViewModel = null;
            //DataContext = null;
            //this.Loaded -= View_Loaded;
            //this.Unloaded -= View_Unloaded;
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!ViewModel.IsMultiSelection)
            {
                FrameworkElement senderElement = sender as FrameworkElement;
                var menu = this.Resources["ContextMenu"] as MenuFlyout;
                var position = e.GetPosition(senderElement);
                menu.ShowAt(senderElement, position);
            }
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var song = (sender as SlidableListItem).DataContext as MusicItem;
            await ViewModel.SlidableListItemLeftCommandRequested(song);
        }

        private void EnableMultipleSelection(object sender, RoutedEventArgs e)
        {
            ViewModel.EnableMultipleSelection();
            selectionButtons.ShowMultipleSelectionButtons();
        }

        private void DisableMultipleSelection(object sender, RoutedEventArgs e)
        {
            ViewModel.DisableMultipleSelection();
            selectionButtons.HideMultipleSelectionButtons();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            GenresListView.SelectAll();
        }
    }
}
