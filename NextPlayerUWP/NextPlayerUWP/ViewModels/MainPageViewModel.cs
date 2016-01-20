using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class MainPageViewModel:Template10.Mvvm.ViewModelBase
    {
        public MainPageViewModel()
        {
            Data = new ObservableCollection<string>();
            for (int i = 0; i < 100; i++)
            {
                Data.Add("element " + i.ToString());
            }
            Item = Data.FirstOrDefault();
        }

        private string item;
        public string Item
        {
            get { return item; }
            set { Set(ref item, value); }
        }

        private ObservableCollection<string> data;
        public ObservableCollection<string> Data
        {
            get { return data; }
            set { Set(ref data, value); }
        }

        private RelayCommand goTo;

        /// <summary>
        /// Gets the Goto.
        /// </summary>
        public RelayCommand Goto
        {
            get
            {
                return goTo
                    ?? (goTo = new RelayCommand(
                    () =>
                    {
                        NavigationService.Navigate(App.Pages.Genres);
                    }));
            }
        }

        private RelayCommand songs;
        public RelayCommand Songs
        {
            get
            {
                return songs
                    ?? (songs = new RelayCommand(
                    () =>
                    {
                        NavigationService.Navigate(App.Pages.Songs);
                    }));
            }
        }
        private RelayCommand albums;
        public RelayCommand Albums
        {
            get
            {
                return albums
                    ?? (albums = new RelayCommand(
                    () =>
                    {
                        NavigationService.Navigate(App.Pages.Albums);
                    }));
            }
        }
        private RelayCommand artists;
        public RelayCommand Artists
        {
            get
            {
                return artists
                    ?? (artists = new RelayCommand(
                    () =>
                    {
                        NavigationService.Navigate(App.Pages.Artists);
                    }));
            }
        }
        private RelayCommand folders;
        public RelayCommand Folders
        {
            get
            {
                return folders
                    ?? (folders = new RelayCommand(
                    () =>
                    {
                        NavigationService.Navigate(App.Pages.Folders);
                    }));
            }
        }
    }
}
