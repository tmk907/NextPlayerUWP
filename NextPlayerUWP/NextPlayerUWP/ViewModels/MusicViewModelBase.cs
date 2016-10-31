using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using NextPlayerUWP.Common;
using Windows.Foundation;
using NextPlayerUWPDataLayer.Services;
using NextPlayerUWPDataLayer.Helpers;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Template10.Services.NavigationService;
using NextPlayerUWPDataLayer.Constants;

namespace NextPlayerUWP.ViewModels
{
    public abstract class MusicViewModelBase:Template10.Mvvm.ViewModelBase
    {
        //public MusicViewModelBase()
        //{
        //    App.MemoryUsageReduced += App_MemoryUsageReduced;
        //}

        private void App_MemoryUsageReduced(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Before: {0}", Windows.System.MemoryManager.AppMemoryUsage);
            FreeResources();
            GC.Collect();
            System.Diagnostics.Debug.WriteLine("After: {0}", Windows.System.MemoryManager.AppMemoryUsage);
        }

        protected ListView listView;
        protected int firstVisibleItemIndex;
        protected string positionKey;
        protected int selectedItemIndex;
        protected bool isBack;
        protected bool onNavigatedCompleted = false;
        protected bool onLoadedCompleted = false;

        protected string pageTitle;
        public string PageTitle
        {
            get { return pageTitle; }
            set { Set(ref pageTitle, value); }
        }

        protected ObservableCollection<ComboBoxItemValue> comboBoxItemValues = new ObservableCollection<ComboBoxItemValue>();
        public ObservableCollection<ComboBoxItemValue> ComboBoxItemValues
        {
            get { return comboBoxItemValues; }
            set
            {
                comboBoxItemValues = value;
            }
        }

        protected ComboBoxItemValue selectedComboBoxItem;
        public ComboBoxItemValue SelectedComboBoxItem
        {
            get { return selectedComboBoxItem; }
            set
            {
                bool diff = selectedComboBoxItem != value;
                Set(ref selectedComboBoxItem, value);
                if (value != null && diff)
                {
                    SortMusicItems();
                }
            }
        }

        protected virtual void SortMusicItems() { }

        #region Commands

        public async void PlayNow(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async void PlayNext(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.AddNext(item);
        }

        public async void AddToNowPlaying(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.Add(item);
        }

        public void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter()); 
        }

        public void Share(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            //NavigationService.Navigate(App.Pages.BluetoothSharePage, item.GetParameter()); TODO
        }

        public async void Pin(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await TileManager.CreateTile(item);
        }

        public void EditTags(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.TagsEditor, item.GetParameter());
        }

        public void ShowDetails(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.FileInfo, ((SongItem)item).GetParameter());
        }

        public async void GoToArtist(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            ArtistItem temp = await DatabaseManager.Current.GetArtistItemAsync(item.FirstArtist);
            App.Current.NavigationService.Navigate(App.Pages.Artist, temp.ArtistId);
        }

        public async void GoToAlbum(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            AlbumItem temp = await DatabaseManager.Current.GetAlbumItemAsync(item.Album, item.AlbumArtist);
            App.Current.NavigationService.Navigate(App.Pages.Album, temp.AlbumId);
        }

        public async Task SlidableListItemLeftCommandRequested(MusicItem item)
        {
            string swipeAction = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ActionAfterSwipeLeftCommand) as string;
            switch (swipeAction)
            {
                case AppConstants.SwipeActionPlayNow:
                    await NowPlayingPlaylistManager.Current.NewPlaylist(item);
                    await PlaybackService.Instance.PlayNewList(0);
                    break;
                case AppConstants.SwipeActionPlayNext:
                    await NowPlayingPlaylistManager.Current.AddNext(item);
                    break;
                case AppConstants.SwipeActionAddToNowPlaying:
                    await NowPlayingPlaylistManager.Current.Add(item);
                    break;
                case AppConstants.SwipeActionAddToPlaylist:
                    NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
                    break;
                default:
                    break;
            }
        }

        #endregion

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            App.ChangeBottomPlayerVisibility(true);
            isBack = false;
            firstVisibleItemIndex = 0;
            selectedItemIndex = 0;
            positionKey = null;

            if (state.ContainsKey(nameof(SelectedComboBoxItem)))
            {
                SelectedComboBoxItem =  ComboBoxItemValues.ElementAt((int)state[nameof(SelectedComboBoxItem)]);
            }
            if (state.Any())
            {
                if (state.ContainsKey(nameof(positionKey)))
                {
                    positionKey = state[nameof(positionKey)] as string;
                }
                if (state.ContainsKey(nameof(firstVisibleItemIndex)))
                {
                    firstVisibleItemIndex = (int)state[nameof(firstVisibleItemIndex)];
                }
                state.Clear();
            }
            if (NavigationMode.Back == mode) isBack = true;
            if (RestoreListPosition(mode))
            {
                var navState = StateManager.Current.Read(this.GetType().ToString());
                if (navState != null && navState.Any())
                {
                    if (navState.ContainsKey(nameof(firstVisibleItemIndex)))
                    {
                        firstVisibleItemIndex = (int)navState[nameof(firstVisibleItemIndex)];
                    }
                    if (navState.ContainsKey(nameof(positionKey)))
                    {
                        positionKey = navState[nameof(positionKey)] as string;
                    }
                    StateManager.Current.Clear(this.GetType().ToString());//is it necessary?
                }
            }

            ChildOnNavigatedTo(parameter, mode, state);

            onNavigatedCompleted = true;
            if (onLoadedCompleted)
            {
                System.Diagnostics.Debug.WriteLine("OnNavigatedToAsync LoadAndScroll()");
                await LoadAndScroll();
            }


            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
        }

        virtual public bool RestoreListPosition(NavigationMode mode)
        {
            if (NavigationMode.Back == mode || NavigationMode.Refresh == mode) return true;
            else return false;
        }

        virtual public void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state) { }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            firstVisibleItemIndex = 0;
            if (listView.ItemsPanelRoot != null)
            {
                positionKey = ListViewPersistenceHelper.GetRelativeScrollPosition(listView, ItemToKeyHandler);

                if (listView.ItemsPanelRoot.GetType() == typeof(ItemsStackPanel))
                {
                    var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
                    firstVisibleItemIndex = isp.FirstVisibleIndex;
                }
                else
                {
                    var isp = (ItemsWrapGrid)listView.ItemsPanelRoot;
                    firstVisibleItemIndex = isp.FirstVisibleIndex;
                }
            }

            Dictionary<string, object> navState = new Dictionary<string, object>();
            navState.Add(nameof(firstVisibleItemIndex), firstVisibleItemIndex);
            navState.Add(nameof(positionKey), positionKey);
            StateManager.Current.Save(this.GetType().ToString(), navState);

            if (suspending)
            {
                state[nameof(firstVisibleItemIndex)] = firstVisibleItemIndex;
                state[nameof(positionKey)] = positionKey;
                if (ComboBoxItemValues.Count > 0)
                {
                    state[nameof(SelectedComboBoxItem)] = ComboBoxItemValues.IndexOf(SelectedComboBoxItem);
                }
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public async void OnLoaded(ListView p)
        {
            listView = p;
            if (onNavigatedCompleted)
            {
                System.Diagnostics.Debug.WriteLine("OnLoaded LoadAndScroll()");
                await LoadAndScroll();
            }
            else onLoadedCompleted = true;//zanim zostanie zmieniona wartosc, w OnNavigatedToAsync moze przeskoczyc do if(onloadedcomplete) ?
        }

        virtual public void FreeResources() { }

        public void OnUnloaded()
        {
            listView = null;
        }

        protected async Task LoadAndScroll()
        {
            await LoadData();
            await SetScrollPosition();

            onNavigatedCompleted = false;
            onLoadedCompleted = false;
        }

        protected virtual async Task LoadData() { await Task.CompletedTask; }

        protected async Task SetScrollPosition()
        {
            if (firstVisibleItemIndex < 0)
            {
                return;
            }
            if(listView.Items.Count <= firstVisibleItemIndex)
            {
                return;
            }
            listView.ScrollIntoView(listView.Items[firstVisibleItemIndex], ScrollIntoViewAlignment.Leading);
            if (positionKey != null)
            {
                await ListViewPersistenceHelper.SetRelativeScrollPositionAsync(listView, positionKey, KeyToItemHandler);
            }
        }

        private string ItemToKeyHandler(object item)
        {
            if (item == null) return null;
            MusicItem mi;
            
            if (item.GetType() == typeof(GroupList))
            {
                mi = (MusicItem)(((GroupList)item)[0]);
            }
            else
            {
                mi = (MusicItem)item;
            }
            return mi.GetParameter();
        }

        private IAsyncOperation<object> KeyToItemHandler(string key)
        {
            return Dispatcher.DispatchAsync(() =>
            {
                if (listView.Items.Count <= 0)
                {
                    return null;
                }
                else
                {
                    var i = listView.Items[firstVisibleItemIndex];
                    if (((MusicItem)i).GetParameter() == key)
                    {
                        return i;
                    }
                    foreach (var item in listView.Items)
                    {
                        if (((MusicItem)item).GetParameter() == key) return item;
                    }
                    return null;
                }
            }).AsAsyncOperation();           
        }

        public void DragStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.RequestedOperation = DataPackageOperation.Copy;
            object item = e.Items.FirstOrDefault();
            e.Data.Properties.Add(item.GetType().ToString(), item);
        }

        private string TimeSpanFormat(TimeSpan span)
        {
            if (span.CompareTo(TimeSpan.Zero) == -1)
            {
                return "0:00";
            }
            if (span.Hours == 0)
            {
                if (span.Duration().Minutes == 0) return "0" + span.ToString(@"\:ss");
                else return span.ToString(@"m\:ss");
            }
            else if (span.Days == 0)
            {
                return span.ToString(@"h\:mm\:ss");
            }
            else
            {
                return span.ToString(@"d\.hh\:mm\:ss");
            }
        }

    }
}
