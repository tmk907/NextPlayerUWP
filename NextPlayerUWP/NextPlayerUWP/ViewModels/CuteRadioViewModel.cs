using Microsoft.Toolkit.Uwp;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Radio.CuteRadio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Xaml.Controls;
using NextPlayerUWP.Common;
using Windows.UI.Xaml;
using NextPlayerUWPDataLayer.Services;
using NextPlayerUWPDataLayer.Radio.CuteRadio.Model;

namespace NextPlayerUWP.ViewModels
{
    public class CuteRadioViewModel : Template10.Mvvm.ViewModelBase
    {
        private CuteRadioService radioService;

        private RadioSourceCountry sourceCountry;
        private RadioSourceGenre sourceGenre;
        private RadioSourceLanguage sourceLanguage;
        private RadioSourceSearch sourceSearch;

        public CuteRadioViewModel()
        {
            radioService = new CuteRadioService();
            sourceCountry = new RadioSourceCountry(radioService);
            sourceGenre = new RadioSourceGenre(radioService);
            sourceLanguage = new RadioSourceLanguage(radioService);
            sourceSearch = new RadioSourceSearch(radioService);
            RadiosByCountry = new IncrementalLoadingCollection<RadioSourceCountry, Station>(sourceCountry, 20, OnStartLoading, OnEndLoading);
            RadiosByGenre = new IncrementalLoadingCollection<RadioSourceGenre, Station>(sourceGenre, 20, OnStartLoading, OnEndLoading);
            RadiosByLanguage = new IncrementalLoadingCollection<RadioSourceLanguage, Station>(sourceLanguage, 20, OnStartLoading, OnEndLoading);
            RadiosSearch = new IncrementalLoadingCollection<RadioSourceSearch, Station>(sourceSearch, 20, OnStartLoading, OnEndLoading);
        }

        public IncrementalLoadingCollection<RadioSourceGenre, Station> RadiosByGenre;
        public IncrementalLoadingCollection<RadioSourceLanguage, Station> RadiosByLanguage;
        public IncrementalLoadingCollection<RadioSourceCountry, Station> RadiosByCountry;
        public IncrementalLoadingCollection<RadioSourceSearch, Station> RadiosSearch;

        //0 country 1 genre 2 lang
        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set { Set(ref selectedPivotIndex, value); }
        }

        public void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(((Pivot)sender).SelectedIndex)
            {
                case 0:
                    ShowCountries();
                    break;
                case 1:
                    ShowGenres();
                    break;
                case 2:
                    ShowLanguages();
                    break;
                case 3:
                    ShowSearch();
                    break;
                default:
                    break;
            }
        }

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { Set(ref loading, value); }
        }

        public void OnStartLoading()
        {
            Loading = true;
        }

        public void OnEndLoading()
        {
            Loading = false;
        }

        private ObservableCollection<string> genres = new ObservableCollection<string>();
        public ObservableCollection<string> Genres
        {
            get { return genres; }
            set { Set(ref genres, value); }
        }

        private ObservableCollection<string> countries = new ObservableCollection<string>();
        public ObservableCollection<string> Countries
        {
            get { return countries; }
            set { Set(ref countries, value); }
        }

        private ObservableCollection<string> languages = new ObservableCollection<string>();
        public ObservableCollection<string> Languages
        {
            get { return languages; }
            set { Set(ref languages, value); }
        }

        private ObservableCollection<RadioItem> streams = new ObservableCollection<RadioItem>();
        public ObservableCollection<RadioItem> Streams
        {
            get { return streams; }
            set { Set(ref streams, value); }
        }

        private bool areStationsVisible = false;
        public bool AreStationsVisible
        {
            get { return areStationsVisible; }
            set { Set(ref areStationsVisible, value); }
        }

        private bool searching = false;
        public bool Searching
        {
            get { return searching; }
            set { Set(ref searching, value); }
        }

        public async void ShowGenres()
        {
            if (genres.Count == 0)
            {
                Loading = true;
                var list = await radioService.GetAllGenres();
                Genres = new ObservableCollection<string>(list);
                Loading = false;
            }
            AreStationsVisible = false;
        }

        public async void ShowLanguages()
        {
            if (languages.Count == 0)
            {
                Loading = true;
                var list = await radioService.GetAllLanguages();
                Languages = new ObservableCollection<string>(list);
                Loading = false;
            }
            AreStationsVisible = false;
        }

        public async void ShowCountries()
        {
            if (countries.Count == 0)
            {
                Loading = true;
                var list = await radioService.GetAllCountries();
                Countries = new ObservableCollection<string>(list);
                Loading = false;
            }
            AreStationsVisible = false;
        }

        public void ShowSearch()
        {
            AreStationsVisible = false;
        }
       
        public async void GenreClick(object sender, ItemClickEventArgs e)
        {
            sourceGenre.Name = e.ClickedItem as string;
            await RadiosByGenre.RefreshAsync();
            AreStationsVisible = true;
        }

        public async void LanguageClick(object sender, ItemClickEventArgs e)
        {
            sourceLanguage.Name = e.ClickedItem as string;
            await RadiosByLanguage.RefreshAsync();
            AreStationsVisible = true;
        }

        public async void CountryClick(object sender, ItemClickEventArgs e)
        {
            sourceCountry.Name = e.ClickedItem as string;
            await RadiosByCountry.RefreshAsync();
            AreStationsVisible = true;
        }

        public async void Search(string name)
        {
            Searching = true;
            sourceSearch.Name = name;
            await RadiosSearch.RefreshAsync();

            Searching = false;
        }

        public async void RadioNameClick(object sender, ItemClickEventArgs e)
        {
            var item = (Station)e.ClickedItem;
            if (item.Source.EndsWith(".m3u") || item.Source.EndsWith(".pls"))
            {
                //download and parse playlist
                RadioItem radio = new RadioItem(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio);
                radio.Name = item.Title;
                radio.StreamUrl = item.Source;
                await NowPlayingPlaylistManager.Current.NewPlaylist(radio);
                await PlaybackService.Instance.PlayNewList(0);
            }
            else if (item.Source.EndsWith(".mp3") || item.Source.EndsWith(".aac"))
            {
                RadioItem radio = new RadioItem(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio);
                radio.Name = item.Title;
                radio.StreamUrl = item.Source;
                await NowPlayingPlaylistManager.Current.NewPlaylist(radio);
                await PlaybackService.Instance.PlayNewList(0);
            }
            else
            {
                RadioItem radio = new RadioItem(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio);
                radio.Name = item.Title;
                radio.StreamUrl = item.Source;
                await NowPlayingPlaylistManager.Current.NewPlaylist(radio);
                await PlaybackService.Instance.PlayNewList(0);
            }
        }

        //public async void RadioStreamClick(object sender, ItemClickEventArgs e)
        //{
        //    var item = (RadioItem)e.ClickedItem;
        //    await NowPlayingPlaylistManager.Current.NewPlaylist(item);
        //    await PlaybackService.Instance.PlayNewList(0);
        //}

        public async void PlayNow(object sender, RoutedEventArgs e)
        {
            var item = (Station)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            var radio = new RadioItem(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio)
            {
                Name = item.Title,
                StreamUrl = item.Source,
            };
            await NowPlayingPlaylistManager.Current.NewPlaylist(radio);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async void PlayNext(object sender, RoutedEventArgs e)
        {
            var item = (Station)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            var radio = new RadioItem(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio)
            {
                Name = item.Title,
                StreamUrl = item.Source,
            };
            await NowPlayingPlaylistManager.Current.AddNext(radio);
        }

        public async void AddToNowPlaying(object sender, RoutedEventArgs e)
        {
            var item = (Station)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            var radio = new RadioItem(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio)
            {
                Name = item.Title,
                StreamUrl = item.Source,
            };
            await NowPlayingPlaylistManager.Current.Add(radio);
        }

        public void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var item = (Station)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            var radio = new RadioItem(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio)
            {
                Name = item.Title,
                StreamUrl = item.Source,
            };
            NavigationService.Navigate(App.Pages.AddToPlaylist, radio.GetParameter());
        }

        public async void AddToFavourites(object sender, RoutedEventArgs e)
        {
            var item = (Station)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            var list = await DatabaseManager.Current.GetRadioFavouritesAsync();
            var radio = list.FirstOrDefault(r => r.BroadcastId == item.Id);
            if (radio == null)
            {
                await DatabaseManager.Current.AddRadioToFavourites(new SimpleRadioData(item.Id, NextPlayerUWPDataLayer.Enums.RadioType.CuteRadio, item.Title, item.Source));
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Search(sender.Text);
        }
    }

    public class RadioSourceGenre : IIncrementalSource<Station>
    {
        private CuteRadioService radioService;
        private readonly List<Station> radios;
        public string Name = "";
        public RadioSourceGenre(CuteRadioService radioService)
        {
            this.radioService = radioService;
            radios = new List<Station>();
        }

        public async Task<IEnumerable<Station>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await radioService.GetStationsByGenre(Name ,pageIndex);
        }
    }

    public class RadioSourceCountry : IIncrementalSource<Station>
    {
        private CuteRadioService radioService;
        private readonly List<Station> radios;
        public string Name = "";
        public RadioSourceCountry(CuteRadioService radioService)
        {
            this.radioService = radioService;
            radios = new List<Station>();
        }

        public async Task<IEnumerable<Station>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await radioService.GetStationsByCountry(Name, pageIndex);
        }
    }

    public class RadioSourceLanguage : IIncrementalSource<Station>
    {
        private CuteRadioService radioService;
        private readonly List<Station> radios;
        public string Name = "";
        public RadioSourceLanguage(CuteRadioService radioService)
        {
            this.radioService = radioService;
            radios = new List<Station>();
        }

        public async Task<IEnumerable<Station>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await radioService.GetStationsByLanguage(Name, pageIndex);
        }
    }

    public class RadioSourceSearch : IIncrementalSource<Station>
    {
        private CuteRadioService radioService;
        private readonly List<Station> radios;
        public string Name = "";
        public RadioSourceSearch(CuteRadioService radioService)
        {
            this.radioService = radioService;
            radios = new List<Station>();
        }

        public async Task<IEnumerable<Station>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Name == "") return new List<Station>();
            return await radioService.SearchStations(Name, pageIndex);
        }
    }
}
