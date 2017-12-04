using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using NextPlayerUWPDataLayer.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Template10.Services.NavigationService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using NextPlayerUWP.Commands;
using NextPlayerUWP.Commands.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public abstract class MusicViewModelBase : Template10.Mvvm.ViewModelBase
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

        protected bool sortDescending = false;
        public bool SortDescending
        {
            get { return sortDescending; }
            set { Set(ref sortDescending, value); }
        }

        protected object persitedItemForConnectedAnimation;

        public void ChangeSortingPriority()
        {
            //sortDescending = !sortDescending;
            SortMusicItems();
        }

        protected virtual void SortMusicItems() { }

        #region Commands

        public async void PlayNow(object sender, RoutedEventArgs e)
        {
            var command = new PlayNowCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void PlayNext(object sender, RoutedEventArgs e)
        {
            var command = new PlayNextCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void AddToNowPlaying(object sender, RoutedEventArgs e)
        {
            var command = new AddToNowPlayingCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void Share(object sender, RoutedEventArgs e)
        {
            var command = new ShareCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void Pin(object sender, RoutedEventArgs e)
        {
            var command = new PinCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void EditTags(object sender, RoutedEventArgs e)
        {
            var command = new EditTagsCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void ShowDetails(object sender, RoutedEventArgs e)
        {
            var command = new ShowDetailsCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var command = new AddToPlaylistCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void GoToArtist(object sender, RoutedEventArgs e)
        {
            var command = new GoToArtistCommand((SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void GoToAlbum(object sender, RoutedEventArgs e)
        {
            var command = new GoToAlbumCommand((SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async Task SlidableListItemLeftCommandRequested(MusicItem item)
        {
            string swipeAction = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand) as string;
            TranslationHelper helper = new TranslationHelper();
            switch (swipeAction)
            {
                case SettingsKeys.SwipeActionPlayNow:
                    await NowPlayingPlaylistManager.Current.NewPlaylist(item);
                    await PlaybackService.Instance.PlayNewList(0);
                    break;
                case SettingsKeys.SwipeActionPlayNext:
                    await NowPlayingPlaylistManager.Current.AddNext(item);
                    string text = helper.GetTranslation(TranslationHelper.AddedNext);
                    MessageHub.Instance.Publish<InAppNotification>(new InAppNotification() { FirstTextLine = text });
                    break;
                case SettingsKeys.SwipeActionAddToNowPlaying:
                    await NowPlayingPlaylistManager.Current.Add(item);
                    string text2 = helper.GetTranslation(TranslationHelper.AddedToNowPlaying);
                    MessageHub.Instance.Publish<InAppNotification>(new InAppNotification() { FirstTextLine = text2 });
                    break;
                case SettingsKeys.SwipeActionAddToPlaylist:
                    NavigationService.Navigate(AppPages.Pages.AddToPlaylist, item.GetParameter());
                    break;
                default:
                    break;
            }
        }

        #endregion

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            App.OnNavigatedToNewView(true);
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
            if (NavigationMode.New == mode)
            {
                persitedItemForConnectedAnimation = null;
            }
            if (CanRestoreListPosition(mode))
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

        virtual public bool CanRestoreListPosition(NavigationMode mode)
        {
            if (NavigationMode.Back == mode || NavigationMode.Refresh == mode) return true;
            else return false;
        }

        virtual public void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state) { }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            firstVisibleItemIndex = 0;
            if (listView == null)
            {

            }
            if (listView != null && listView.ItemsPanelRoot != null)
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
            DisableMultipleSelection();
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
            await StartConnectedAnimation();

            onNavigatedCompleted = false;
            onLoadedCompleted = false;
        }

        protected virtual async Task LoadData() { await Task.CompletedTask; }
        protected virtual async Task StartConnectedAnimation() { await Task.CompletedTask; }

        protected async Task SetScrollPosition()
        {
            if (firstVisibleItemIndex < 0)
            {
                return;
            }
            if(listView == null || listView.Items.Count <= firstVisibleItemIndex)
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

        private bool isClickEnabled = true;
        public bool IsClickEnabled
        {
            get { return isClickEnabled; }
            set
            {
                Set(ref isClickEnabled, value);
                IsMultiSelection = !isClickEnabled && selectionMode == ListViewSelectionMode.Multiple;
            }
        }

        private ListViewSelectionMode selectionMode = ListViewSelectionMode.None;
        public ListViewSelectionMode SelectionMode
        {
            get { return selectionMode; }
            set
            {
                Set(ref selectionMode, value);
                IsMultiSelection = !isClickEnabled && selectionMode == ListViewSelectionMode.Multiple;
            }
        }

        private bool isMultiSelection = false;
        public bool IsMultiSelection
        {
            get { return isMultiSelection; }
            set { Set(ref isMultiSelection, value); }
        }

        public void EnableMultipleSelection()
        {
            IsClickEnabled = false;
            SelectionMode = ListViewSelectionMode.Multiple;
        }

        public void DisableMultipleSelection()
        {
            IsClickEnabled = true;
            SelectionMode = ListViewSelectionMode.None;
        }

        public async Task PlayNowMany(IEnumerable<MusicItem> items)
        {
            DisableMultipleSelection();
            await NowPlayingPlaylistManager.Current.NewPlaylist(items);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async Task PlayNextMany(IEnumerable<MusicItem> items)
        {
            DisableMultipleSelection();
            await NowPlayingPlaylistManager.Current.AddNext(items);
        }

        public async Task AddToNowPlayingMany(IEnumerable<MusicItem> items)
        {
            DisableMultipleSelection();
            await NowPlayingPlaylistManager.Current.Add(items);
        }

        public void AddToPlaylistMany(IEnumerable<MusicItem> items)
        {
            App.AddToCache(items);
            DisableMultipleSelection();
            NavigationService.Navigate(AppPages.Pages.AddToPlaylist, new ListOfMusicItems().GetParameter());
        }

        public async void ShareMany(IEnumerable<MusicItem> items)
        {
            var command = new ShareCommand(items);
            await command.Excecute();
            DisableMultipleSelection();
        }

    }
}
