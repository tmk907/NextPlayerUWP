using NextPlayerWinUI.ViewModels;
using System;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerWinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LyricsView : Page
    {
        LyricsViewModel ViewModel;

        public LyricsView()
        {
            this.InitializeComponent();
            ViewModel = (LyricsViewModel)DataContext;
        }

        private async void SearchButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialogSearchLyrics.XamlRoot = XamlRoot;
            await ContentDialogSearchLyrics.ShowAsync();
        }

        private void ContentDialogSearchLyrics_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.ArtistSearch = ArtistSearch.Text;
            ViewModel.TitleSearch = TitleSearch.Text;
            ViewModel.SearchLyrics();
        }
        //~LyricsView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

    }
}
