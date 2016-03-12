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
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Keys = new ObservableCollection<string>();
                Keys.Add("25.04.2014");
                Keys.Add("5.04.2016");
            }
        }

        private ObservableCollection<string> keys = new ObservableCollection<string>();
        public ObservableCollection<string> Keys
        {
            get { return keys; }
            set { Set(ref keys, value); }
        }
    }
}
