﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumArtistView : Page
    {
        public AlbumArtistViewModel ViewModel;
        public AlbumArtistView()
        {
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (AlbumArtistViewModel)DataContext;
        }

        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            ShuffleAppBarButton.Click -= ShuffleAppBarButton_Click;
            ViewModel = null;
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ShuffleAppBarButton.Click += ShuffleAppBarButton_Click;
            ViewModel.OnLoaded(AlbumArtistSongsListView);
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private void AlbumGroupHeader_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["AlbumContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var song = (sender as SlidableListItem).DataContext as MusicItem;
            await ViewModel.SlidableListItemLeftCommandRequested(song);
        }

        private void AlbumCoverImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            image.Fade(1, 500, 0).Start();
        }

        private void ShuffleAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShuffleAllSongs();
        }
    }
}