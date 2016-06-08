using NextPlayerUWP.Common;
using NextPlayerUWP.Helpers;
using NextPlayerUWPDataLayer.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWP.ViewModels
{
    //public class PlaylistRule : Template10.Mvvm.BindableBase
    //{
    //    private ResourceLoader loader;
    //    public PlaylistRule(ResourceLoader resLoader)
    //    {
    //        loader = resLoader;
    //        Items = new List<ComboBoxItemValue>();
    //        foreach (var k in typeof(SPUtility.Item).GetRuntimeFields())
    //        {
    //            object o = k.GetValue(null);
    //            if (o is string)
    //            {
    //                Items.Add(new ComboBoxItemValue(loader.GetString(o as string), o as string));
    //            }
    //        }
    //    }

    //    public List<ComboBoxItemValue> Items { get; }

    //    public string selectedItem = "";
    //    public string SelectedItem
    //    {
    //        get { return selectedItem; }
    //        set { Set(ref selectedItem, value); }
    //    }

    //    public string selectedComparison = "";
    //    public string SelectedComparison
    //    {
    //        get { return selectedComparison; }
    //        set { Set(ref selectedComparison, value); }
    //    }

    //    public string value = "";
    //    public string Value
    //    {
    //        get { return value; }
    //        set { Set(ref value, value); }
    //    }

    //    public string selectedBoolOperator = "";
    //    public string SelectedBoolOperator
    //    {
    //        get { return selectedBoolOperator; }
    //        set { Set(ref selectedBoolOperator, value); }
    //    }

    //    private bool isDatePickerVisible = false;
    //    public bool IsDatePickerVisible
    //    {
    //        get { return isDatePickerVisible; }
    //        set { Set(ref isDatePickerVisible, value); }
    //    }

    //    private bool isTimesUnitsVisible = false;
    //    public bool IsTimesUnitsVisible
    //    {
    //        get { return isTimesUnitsVisible; }
    //        set { Set(ref isTimesUnitsVisible, value); }
    //    }

    //}

    public class NewSmartPlaylistViewModel : Template10.Mvvm.ViewModelBase
    {
        ResourceLoader resLoader;

        public NewSmartPlaylistViewModel()
        {
            resLoader = new ResourceLoader();
            PlaylistRules.Add(new PlaylistRule(resLoader));
        }

        public string playlistName = "";
        public string PlaylistName
        {
            get { return playlistName; }
            set { Set(ref playlistName, value); }
        }

        public int maxSongsNumber = 100;
        public int MaxSongsNumber
        {
            get { return maxSongsNumber; }
            set { Set(ref maxSongsNumber, value); }
        }

        private ObservableCollection<ComboBoxItemValue> sortRules = new ObservableCollection<ComboBoxItemValue>();
        public ObservableCollection<ComboBoxItemValue> SortRules
        {
            get
            {
                if (sortRules.Count == 0)
                {
                    foreach (var k in typeof(SPUtility.SortBy).GetRuntimeFields())
                    {
                        object o = k.GetValue(null);
                        if (o is string)
                        {
                            sortRules.Add(new ComboBoxItemValue(o as string, resLoader.GetString(o as string)));
                        }
                    }
                }
                
                return sortRules;
            }
            set { Set(ref sortRules, value); }
        }

        private ComboBoxItemValue selectedSortRule;
        public ComboBoxItemValue SelectedSortRule
        {
            get { return selectedSortRule; }
            set { Set(ref selectedSortRule, value); }
        }

        private ObservableCollection<PlaylistRule> playlistRules = new ObservableCollection<PlaylistRule>();
        public ObservableCollection<PlaylistRule> PlaylistRules
        {
            get { return playlistRules; }
            set { Set(ref playlistRules, value); }
        }

        public void AddRule()
        {
            PlaylistRules.Add(new PlaylistRule(resLoader));
        }

        public void SavePlaylist()
        {
            bool isCorrect = true;
            foreach(var rule in playlistRules)
            {
                if (!rule.IsCorrect())
                {
                    isCorrect = false;
                    break;
                }
            }
            if (isCorrect)
            {

            }
            else
            {

            }
        }
    }
}
