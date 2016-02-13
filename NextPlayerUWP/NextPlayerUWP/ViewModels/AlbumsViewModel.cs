﻿using GalaSoft.MvvmLight.Command;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class AlbumsViewModel : MusicViewModelBase
    {
        CancellationTokenSource tokenSource2;

        public AlbumsViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.artist);
            ComboBoxItemValues = si.GetSortNames();
        }
        private ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();
        public ObservableCollection<AlbumItem> Albums
        {
            get { return albums; }
            set { Set(ref albums, value); }
        }

        private ObservableCollection<GroupList> groupedAlbums = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedAlbums
        {
            get { return groupedAlbums; }
            set { Set(ref groupedAlbums, value); }
        }

        protected override async Task LoadData()
        {
            if (albums.Count == 0)
            {
                Albums = await DatabaseManager.Current.GetAlbumItemsAsync();
            }
            if (groupedAlbums.Count == 0)
            {
                var query = from item in albums
                            orderby item.Album.ToLower()
                            group item by item.Album[0].ToString().ToLower() into g
                            orderby g.Key
                            select new { GroupName = g.Key.ToUpper(), Items = g };
                ObservableCollection<GroupList> gr = new ObservableCollection<GroupList>();
                foreach (var g in query)
                {
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        group.Add(item);
                    }
                    gr.Add(group);
                }
                GroupedAlbums = gr;
            }

            tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;
            await Task.Run(() => CheckCovers(ct));
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            tokenSource2.Cancel();
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Album, ((AlbumItem)e.ClickedItem).GetParameter());
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItemValue value = (ComboBoxItemValue)e.AddedItems.FirstOrDefault();
            switch (value.Option)
            {
                case SortNames.Album:
                    Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower(), "Album");
                    break;
                case SortNames.AlbumArtist:
                    Sort(s => s.AlbumArtist, t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower(), "AlbumArtist");
                    break;
                case SortNames.Year:
                    Sort(s => s.Year, t => t.Year, "AlbumId");
                    break;
                case SortNames.Duration:
                    Sort(s => s.Duration, t => t.Duration, "AlbumId");
                    break;
                case SortNames.SongCount:
                    Sort(s => s.Duration, t => t.Duration, "AlbumId");
                    break;
                default:
                    Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower(), "Album");
                    break;
            }
        }

        private void Sort(Func<AlbumItem, object> orderSelector, Func<AlbumItem, object> groupSelector, string propertyName)
        {
            var query = albums.OrderBy(orderSelector).
                GroupBy(groupSelector).
                OrderBy(g => g.Key).
                Select(group => new { GroupName = group.Key.ToString().ToUpper(), Items = group });
            int i = 0;
            string s;
            GroupedAlbums.Clear();
            foreach (var g in query)
            {
                i = 0;
                s = "";
                GroupList group = new GroupList();
                group.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    string prop = item.GetType().GetProperty(propertyName).GetValue(item, null).ToString();
                    if (group.Count != 0 && prop != s) i++;
                    item.Index = i;
                    s = prop;
                    group.Add(item);
                }
                GroupedAlbums.Add(group);
            }
        }

        public async Task CheckCovers(CancellationToken ct)
        {
            //foreach(var group in GroupedAlbums)
            //{
                foreach(AlbumItem album in albums)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    if (!album.IsImageSet)
                    {
                        await Dispatcher.DispatchAsync(async() => {

                            string path = await ImagesManager.GetAlbumCoverPath(album);
                            album.ImagePath = path;
                            album.ImageUri = new Uri(path);
                            album.IsImageSet = true;
                        });
                        await DatabaseManager.Current.UpdateAlbumItem(album);
                    }
                }
                
            //}
            //foreach (AlbumItem album in albums)
            //{
            //    if (ct.IsCancellationRequested)
            //    {
            //        break;
            //    }
            //    if (album.ImagePath == "")
            //    {
            //        string path = await Dispatcher.DispatchAsync(() => ImagesManager.GetAlbumCoverPath(album)).Result;
            //        //string path = await ImagesManager.GetAlbumCoverPath(album);
            //        //Dispatcher.Dispatch(() => {
            //            album.ImagePath = path;
            //        //});
            //        await DatabaseManager.Current.UpdateAlbumItem(album);
            //    }
            //}
        }

        //public void CheckCovers()
        //{
        //    //var albums = DatabaseManager.Current.GetAlbumItemsAsync().RunSynchronously();
        //    IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
        //    async (workItem) =>
        //    {
        //        foreach (AlbumItem album in albums)
        //        {
        //            if (workItem.Status == AsyncStatus.Canceled)
        //            {
        //                break;
        //            }
        //            if (album.ImagePath == "")
        //            {
        //                string path = await ImagesManager.GetAlbumCoverPath(album);
        //                Dispatcher.Dispatch(() => {
        //                    album.ImagePath = path;
        //                });
        //                await DatabaseManager.Current.UpdateAlbumItem(album);
        //            }
        //        }

        //    });

        //    m_workItem = asyncAction;
        //    asyncAction.Completed = new AsyncActionCompletedHandler(
        //        (IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
        //        {
        //            if (asyncStatus == AsyncStatus.Canceled)
        //            {
        //                return;
        //            }

        //            // Update the UI thread with the CoreDispatcher. 
        //            Dispatcher.DispatchAsync(() =>
        //            {

        //            });
        //        }
        //    );
        //}
    }
}
