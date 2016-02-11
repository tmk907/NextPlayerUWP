using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class TestViewModel : Template10.Mvvm.ViewModelBase
    {
        public TestViewModel()
        {
            songs = new ObservableCollection<SongItem>();
            for (int i = 1; i < 10; i++)
            {
                SongItem s = new SongItem();
                Songs.Add(s);
            }
        }

        private ObservableCollection<SongItem> songs;
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }
    }
}
