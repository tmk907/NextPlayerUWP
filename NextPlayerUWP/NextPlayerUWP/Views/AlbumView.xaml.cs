﻿using NextPlayerUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Animations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumView : Page
    {
        public AlbumViewModel ViewModel;
        public AlbumView()
        {
            this.InitializeComponent();
            this.Loaded += delegate { ((AlbumViewModel)DataContext).OnLoaded(AlbumSongsListView); };
            ViewModel = (AlbumViewModel)DataContext;
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void EditAlbum_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditAlbum();
            await ContentDialogEditAlbum.ShowAsync();
        }

        private void AlbumCoverImageBG_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            image.Fade(1, 700, 0).Start();
        }

        private void AlbumCoverImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            image.Fade(1, 800, 0).Start();
        }
    }
}
