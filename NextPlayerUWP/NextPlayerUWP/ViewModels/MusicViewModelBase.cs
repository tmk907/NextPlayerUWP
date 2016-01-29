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

namespace NextPlayerUWP.ViewModels
{
    public abstract class MusicViewModelBase:Template10.Mvvm.ViewModelBase
    {
        protected ListView listView;
        protected int firstVisibleItemIndex;
        protected string positionKey;
        protected int selectedItemIndex;
        protected bool isBack;
        protected bool onNavigatedCompleted = false;
        protected bool onLoadedCompleted = false;


        protected MusicItem selectedItem;
        public MusicItem SelectedItem
        {
            get { return selectedItem; }
            set { Set(ref selectedItem, value); }
        }

        protected string pageTitle;
        public string PageTitle
        {
            get { return pageTitle; }
            set { Set(ref pageTitle, value); }
        }

        #region Commands
        public async void PlayNow(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.NewPlaylist(item);
            ApplicationSettingsHelper.SaveSongIndex(0);
            PlaybackManager.Current.PlayNew();
        }

        public async void PlayNext(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.AddNext(item);
        }

        public async void AddToNowPlaying(object sender, RoutedEventArgs e)
        {
            //SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.Add(item);
        }

        public void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            //NavigationService.Navigate(App.Pages.AddToPlaylist, SelectedItem.GetParameter()); 
        }

        public void Share(object sender, RoutedEventArgs e)
        {
            SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            //NavigationService.Navigate(App.Pages.BluetoothSharePage, item.GetParameter()); TODO
        }

        public async void Pin(object sender, RoutedEventArgs e)
        {
            await TileManager.CreateTile((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
        }


        #endregion

        ////?
        //public void EditTags(object sender, RoutedEventArgs e)
        //{
        //    SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
        //    NavigationService.Navigate(App.Pages.TagsEditorPage, item.SongId);
        //}

        ////?
        //public void ShowDetails(object sender, RoutedEventArgs e)
        //{
        //    SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
        //    NavigationService.Navigate(App.Pages.FileInfoPage, );
        //}
        
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            isBack = false;
            firstVisibleItemIndex = 0;
            selectedItemIndex = 0;
            positionKey = null;
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
            if (mode == NavigationMode.Back)
            {
                isBack = true;

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
                LoadAndScroll();
            }
            return Task.CompletedTask;
        }

        virtual public void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state) { }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            positionKey = ListViewPersistenceHelper.GetRelativeScrollPosition(listView, ItemToKeyHandler);
            var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
            firstVisibleItemIndex = isp.FirstVisibleIndex;

            Dictionary<string, object> navState = new Dictionary<string, object>();
            navState.Add(nameof(firstVisibleItemIndex), firstVisibleItemIndex);
            navState.Add(nameof(positionKey), positionKey);
            StateManager.Current.Save(this.GetType().ToString(), navState);

            if (suspending)
            {
                state[nameof(firstVisibleItemIndex)] = firstVisibleItemIndex;
                state[nameof(positionKey)] = positionKey;
            }
            return Task.CompletedTask;
        }

        public void OnLoaded(ListView p)
        {
            listView = (ListView)p;
            if (onNavigatedCompleted) LoadAndScroll();
            else onLoadedCompleted = true;//zanim zostanie zmieniona wartosc, w OnNavigatedToAsync moze przeskoczyc do if(onloadedcomplete) ?
        }

        protected async Task LoadAndScroll()
        {
            await LoadData();
            await SetScrollPosition();

            onNavigatedCompleted = false;
            onLoadedCompleted = false;
        }

        protected virtual async Task LoadData() { }

        protected async Task SetScrollPosition()
        {
            //SemanticZoomLocation loc = new SemanticZoomLocation();
            ////listView.SelectedIndex = selectedItemIndex;
            ////loc.Item = listView.SelectedIndex;
            //listView.UpdateLayout();
            //listView.MakeVisible(loc);
            listView.ScrollIntoView(listView.Items[firstVisibleItemIndex], ScrollIntoViewAlignment.Leading);
            if (positionKey != null)
            {
                await ListViewPersistenceHelper.SetRelativeScrollPositionAsync(listView, positionKey, KeyToItemHandler);
            }
        }

        private string ItemToKeyHandler(object item)
        {
            if (item == null) return null;
            return ((MusicItem)item).GetParameter();
        }

        private IAsyncOperation<object> KeyToItemHandler(string key)
        {
            return Task.Run(() =>
            {
                if (listView.Items.Count <= 0)
                {
                    return null;
                }
                else
                {
                    if (((MusicItem)listView.Items[firstVisibleItemIndex]).GetParameter() == key)
                    {
                        return listView.Items[firstVisibleItemIndex];
                    }
                    foreach (var item in listView.Items)
                    {
                        if (((MusicItem)item).GetParameter() == key) return item;
                    }
                    return null;
                }
            }).AsAsyncOperation();
        }
    }
}
