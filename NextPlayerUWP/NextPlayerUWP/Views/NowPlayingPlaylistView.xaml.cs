﻿using NextPlayerUWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingPlaylistView : Page
    {
        public NowPlayingPlaylistViewModel ViewModel;

        public NowPlayingPlaylistView()
        {
            System.Diagnostics.Debug.WriteLine(GetType().Name + "()");
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (NowPlayingPlaylistViewModel)DataContext;
        }
        //~NowPlayingPlaylistView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(GetType().Name+"Unloaded");
            ViewModel.OnUnloaded();           
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(GetType().Name+"Loaded");
            ViewModel.OnLoaded(NowPlayingPlaylistListView);
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

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            NowPlayingPlaylistListView.SelectAll();
        }
    }
}
