using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class ArtistViewModel : MusicViewModelBase
    {
        private string artistParam;

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
            if (parameter != null)
            {
                try
                {
                    var s = parameter.ToString().Split(new string[] { MusicItem.separator }, StringSplitOptions.None);
                    artistParam = s[1];
                }
                catch(Exception ex)
                {

                }
            }
        }

        protected override async Task LoadData()
        {

        }


    }
}
