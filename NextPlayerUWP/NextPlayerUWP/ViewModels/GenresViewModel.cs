﻿using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NextPlayerUWP.Common;

namespace NextPlayerUWP.ViewModels
{
    public class GenresViewModel : MusicViewModelBase
    {
        public GenresViewModel()
        {
            sortingHelper = new SortingHelperForGenreItems("Genres");
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            SortDescending = sortingHelper.SortDescending;
            App.SongUpdated += App_SongUpdated;
            MediaImport.MediaImported += MediaImport_MediaImported;
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) Genres.Add(new GenreItem());
        }

        SortingHelperForGenreItems sortingHelper;

        private async void MediaImport_MediaImported(string s)
        {
            Template10.Common.IDispatcherWrapper d = Dispatcher;
            if (d == null)
            {
                d = Template10.Common.WindowWrapper.Current().Dispatcher;
            }
            if (d == null)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("GenresViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                TelemetryAdapter.TrackEvent("Dispatcher null");
                return;
            }
            await d.DispatchAsync(() => ReloadData());
        }

        private ObservableCollection<GenreItem> genres = new ObservableCollection<GenreItem>();
        public ObservableCollection<GenreItem> Genres
        {
            get { return genres; }
            set { Set(ref genres, value); }
        }

        protected override async Task LoadData()
        {
            if (genres.Count == 0)
            {
                Genres = await DatabaseManager.Current.GetGenreItemsAsync();
                SortMusicItems();
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            //SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault();
        }

        public override void FreeResources()
        {
            genres = null;
            genres = new ObservableCollection<GenreItem>();
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(AppPages.Pages.Playlist, ((GenreItem)e.ClickedItem).GetParameter());
        }

        private async void App_SongUpdated(int id)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        private async Task ReloadData()
        {
            Genres = await DatabaseManager.Current.GetGenreItemsAsync();
            SortMusicItems();
        }

        protected override void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            sortingHelper.SortDescending = sortDescending;
            var orderSelector = sortingHelper.GetOrderBySelector();
            var query = (sortDescending) ? genres.OrderByDescending(orderSelector) : genres.OrderBy(orderSelector);
            Genres = new ObservableCollection<GenreItem>(query);
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var m1 = genres.Where(s => s.Genre.ToLower().StartsWith(query));
                var m2 = genres.Where(s => s.Genre.ToLower().Contains(query));
                var result = m1.Concat(m2).Distinct();
                sender.ItemsSource = result.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int index;
            if (args.ChosenSuggestion != null)
            {
                index = genres.IndexOf((GenreItem)args.ChosenSuggestion);
            }
            else
            {
                var list = genres.Where(s => s.Genre.ToLower().StartsWith(sender.Text.ToLower())).OrderBy(s => s.Genre).ToList();
                if (list.Count == 0) return;
                index = 0;
                bool find = false;
                foreach(var g in genres)
                {
                    if (g.Genre.Equals(list.FirstOrDefault().Genre))
                    {
                        find = true;
                        break;
                    }
                    index++;
                }
                if (!find) return;
            }
            
            listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as GenreItem;
            sender.Text = item.Genre;
        }
    }
}
