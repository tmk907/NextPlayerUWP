﻿using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class SongsViewModel : MusicViewModelBase
    {
        public SongsViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.song);
            ComboBoxItemValues = si.GetSortNames();
        }

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    for(int i = 0; i < 10; i++)
                    {
                        Songs.Add(new SongItem());
                    }
                }
                return songs; }
            set { Set(ref songs, value); }
        }

        private ObservableCollection<GroupList> groupedSongs = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedSongs
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    string[] t = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "K" };
                    for (int i = 0; i < 10; i++)
                    {
                        GroupList group = new GroupList();
                        group.Key = t[i];
                        groupedSongs.Add(group);
                    }
                }
                return groupedSongs;
            }
            set { Set(ref groupedSongs, value); }
        }

        protected override async Task LoadData()
        {               
            if (songs.Count == 0)
            {
                Songs = await DatabaseManager.Current.GetSongItemsAsync();
            }
            if (groupedSongs.Count == 0)
            {
                var query = from item in songs
                            orderby item.Title.ToLower()
                            group item by item.Title[0].ToString().ToLower() into g
                            orderby g.Key
                            select new { GroupName = g.Key.ToUpper(), Items = g };
                int i = 0;
                foreach (var g in query)
                {
                    i = 0;
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        item.Index = i;
                        i++;
                        group.Add(item);
                    }
                    GroupedSongs.Add(group);
                }

                //var characterGroupings = new Windows.Globalization.Collation.CharacterGroupings();
                //foreach (var c in characterGroupings)
                //{
                //    GroupedSongs.Add(new GroupList(c.Label));
                //}
                //foreach (var item in songs)
                //{
                //    string a = characterGroupings.Lookup(item.Title);
                //    GroupedSongs.FirstOrDefault(e => e.Key.Equals(a)).Add(item);
                //}
            }
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            foreach(var s in songs)
            {
                if (s.SongId == ((SongItem)e.ClickedItem).SongId) break;
                index++;
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(songs);
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItemValue value = (ComboBoxItemValue)e.AddedItems.FirstOrDefault();
            switch (value.Option)
            {
                case SortNames.Title:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
                case SortNames.Album:
                    Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower(), "Album");
                    break;
                case SortNames.Artist:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
                case SortNames.AlbumArtist:
                    Sort(s => s.AlbumArtist, t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower(), "AlbumArtist");
                    break;
                case SortNames.Year:
                    Sort(s => s.Year, t => t.Year, "SongId");
                    break;
                case SortNames.Duration:
                    Sort(s => s.Duration, t => t.Duration, "SongId");
                    break;
                case SortNames.Rating:
                    Sort(s => s.Rating, t => t.Rating, "SongId");
                    break;
                case SortNames.Composer:
                    Sort(s => s.Composer, t => (t.Composer == "") ? "" : t.Composer[0].ToString().ToLower(), "Composer");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.DateAdded, t => t.DateAdded, "SongId");
                    break;
                case SortNames.LastPlayed:
                    Sort(s => s.LastPlayed, t => t.LastPlayed, "SongId");
                    break;
                case SortNames.PlayCount:
                    Sort(s => s.PlayCount, t => t.PlayCount, "SongId");
                    break;
                default:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
            }
        }

        private void Sort(Func<SongItem, object> orderSelector, Func<SongItem,object> groupSelector, string propertyName)
        {
            var query = songs.OrderBy(orderSelector).ThenBy(a => a.Title).
                GroupBy(groupSelector).
                OrderBy(g => g.Key).
                Select(group => new { GroupName = group.Key.ToString().ToUpper(), Items = group });
            int i = 0;
            string s;
            GroupedSongs.Clear();
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
                GroupedSongs.Add(group);
            }
        }
    }
}
