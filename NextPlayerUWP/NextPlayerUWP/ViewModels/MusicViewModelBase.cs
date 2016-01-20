using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public abstract class MusicViewModelBase:Template10.Mvvm.ViewModelBase
    {
        protected ListView listView;
        protected int firstVisibleItemIndex;
        protected int selectedItemIndex;
        protected bool isBack;


        protected MusicItem selectedItem;
        public MusicItem SelectedItem
        {
            get { return selectedItem; }
            set { Set(ref selectedItem, value); }
        }

        #region Commands
        protected RelayCommand<MusicItem> addToNowPlaying;
        public RelayCommand<MusicItem> AddToNowPlaying
        {
            get
            {
                return addToNowPlaying
                    ?? (addToNowPlaying = new RelayCommand<MusicItem>(
                    item =>
                    {
                        SelectedItem = item;
                        //Library.Current.AddToNowPlaying(item); TODO
                    }));
            }
        }

        protected RelayCommand<MusicItem> addToPlaylist;
        public RelayCommand<MusicItem> AddToPlaylist
        {
            get
            {
                return addToPlaylist
                    ?? (addToPlaylist = new RelayCommand<MusicItem>(
                    item =>
                    {
                        SelectedItem = item;
                        //NavigationService.Navigate(App.Pages.AddToPlaylistPage, item.GetParameter()); TODO
                    }));
            }
        }

        protected RelayCommand<MusicItem> share;
        public RelayCommand<MusicItem> Share
        {
            get
            {
                return share
                    ?? (share = new RelayCommand<MusicItem>(
                    item =>
                    {
                        SelectedItem = item;
                        //NavigationService.Navigate(App.Pages.BluetoothSharePage, item.GetParameter()); TODO
                    }));
            }
        }

        #endregion

        ////?
        //protected RelayCommand<MusicItem> editTags;
        //public RelayCommand<MusicItem> EditTags
        //{
        //    get
        //    {
        //        return editTags
        //            ?? (editTags = new RelayCommand<MusicItem>(
        //            item =>
        //            {
        //                SelectedItem = item;
        //                NavigationService.Navigate(App.Pages.TagsEditorPage, item.SongId);
        //            }));
        //    }
        //}

        ////?
        //protected RelayCommand<MusicItem> showDetails;
        //public RelayCommand<MusicItem> ShowDetails
        //{
        //    get
        //    {
        //        return showDetails
        //            ?? (showDetails = new RelayCommand<MusicItem>(
        //            item =>
        //            {
        //                SelectedItem = item;
        //                NavigationService.Navigate(App.Pages.FileInfoPage, );
        //            }));
        //    }
        //}

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            isBack = false;
            firstVisibleItemIndex = 0;
            selectedItemIndex = 0;

            if (state.Any())
            {
                if (state.ContainsKey("selectedItemIndex"))
                {
                    selectedItemIndex = (int)state["selectedItemIndex"];
                }
                if (state.ContainsKey("firstVisibleItemIndex"))
                {
                    firstVisibleItemIndex = (int)state["firstVisibleItemIndex"];
                }
                state.Clear();
            }
            if (mode == NavigationMode.Back)
            {
                isBack = true;
            }
            
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            selectedItemIndex = listView.SelectedIndex;
            //var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
            //firstVisibleItemIndex = isp.FirstVisibleIndex;
            firstVisibleItemIndex = 0;
            if (suspending)
            {
                state["selectedItemIndex"] = selectedItemIndex;
                state["firstVisibleItemIndex"] = firstVisibleItemIndex;
            }
            return base.OnNavigatedFromAsync(state, suspending);
        }

        protected RelayCommand<object> onLoad;
        public RelayCommand<object> OnLoad
        {
            get
            {
                return onLoad
                    ?? (onLoad = new RelayCommand<object>(
                    p =>
                    {
                        listView = (ListView)p;
                        LoadAndScroll();
                    }));
            }
        }

        public void OnLoaded(ListView p)
        {
            listView = (ListView)p;
            LoadAndScroll();
        }

        protected async Task LoadAndScroll()
        {
            await LoadData();
            SetScrollPosition();
        }

        protected virtual async Task LoadData() { }

        protected void SetScrollPosition()
        {
            SemanticZoomLocation loc = new SemanticZoomLocation();
            listView.SelectedIndex = selectedItemIndex;
            loc.Item = listView.SelectedIndex;
            listView.UpdateLayout();
            listView.MakeVisible(loc);
            listView.ScrollIntoView(listView.Items[firstVisibleItemIndex], ScrollIntoViewAlignment.Leading);
        }

        
    }
}
