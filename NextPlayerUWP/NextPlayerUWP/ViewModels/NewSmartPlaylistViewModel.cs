using NextPlayerUWP.Common;
using NextPlayerUWP.Helpers;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using NextPlayerUWPDataLayer.Model;

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
        int id = -1;
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

        public string maxSongsNumber = "100";
        public string MaxSongsNumber
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

        private bool errorVisibility = false;
        public bool ErrorVisibility
        {
            get { return errorVisibility; }
            set { Set(ref errorVisibility, value); }
        }

        public void AddRule()
        {
            PlaylistRules.LastOrDefault().IsBoolOperatorsVisible = true;
            var rule = new PlaylistRule(resLoader);
            PlaylistRules.Add(rule);
            ErrorVisibility = false;
        }

        public void DeleteRule()
        {
            if (playlistRules.Count > 1)
            {
                PlaylistRules.Remove(playlistRules.LastOrDefault());
                PlaylistRules.LastOrDefault().IsBoolOperatorsVisible = false;
                ErrorVisibility = false;
            }
        }

        public async void SavePlaylist()
        {
            PlaylistRules.LastOrDefault().SelectedBoolOperator = PlaylistRules.LastOrDefault().BoolOperators.FirstOrDefault().Option;
            bool isCorrect = true;

            if (playlistName == "")
            {
                isCorrect = false;
            }
            int songsNumber = 0;
            if (!Int32.TryParse(maxSongsNumber,out songsNumber) && songsNumber < 1)
            {
                isCorrect = false;
            }

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
                if (id == -1)
                {
                    id = await DatabaseManager.Current.InsertSmartPlaylistAsync(playlistName, songsNumber, selectedSortRule.Option);
                }
                else
                {
                    await DatabaseManager.Current.DeleteSmartPlaylistEntries(id);
                }

                foreach (var rule in playlistRules)
                {
                    switch (SPUtility.Item.GetItemType(rule.SelectedItem))
                    {
                        case SPUtility.ItemType.Date:
                            if (rule.SelectedComparison == SPUtility.Comparison.IsGreater) // after 23:59:59
                            {
                                DateTime high = new DateTime(rule.SelectedDate.Year, rule.SelectedDate.Month, rule.SelectedDate.Day);
                                high = new DateTime(high.AddDays(1).Ticks - 1);
                                await DatabaseManager.Current.InsertSmartPlaylistEntryAsync(id, rule.SelectedItem, rule.SelectedComparison, high.Ticks.ToString(), rule.SelectedBoolOperator);
                            }
                            else if (rule.SelectedComparison == SPUtility.Comparison.IsLess)// before 0:00:00
                            {
                                DateTime low = new DateTime(rule.SelectedDate.Year, rule.SelectedDate.Month, rule.SelectedDate.Day);
                                if (rule.SelectedItem == SPUtility.Item.LastPlayed)
                                {
                                    await DatabaseManager.Current.InsertSmartPlaylistEntryAsync(id, rule.SelectedItem, rule.SelectedComparison, low.Ticks.ToString(), SPUtility.Operator.And);
                                    await DatabaseManager.Current.InsertSmartPlaylistEntryAsync(id, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "0", rule.SelectedBoolOperator);
                                }
                                else
                                {
                                    await DatabaseManager.Current.InsertSmartPlaylistEntryAsync(id, rule.SelectedItem, rule.SelectedComparison, low.Ticks.ToString(), rule.SelectedBoolOperator);
                                }
                            }
                            break;
                        case SPUtility.ItemType.Number:
                            await DatabaseManager.Current.InsertSmartPlaylistEntryAsync(id, rule.SelectedItem, rule.SelectedComparison, rule.UserInput, rule.SelectedBoolOperator);
                            break;
                        case SPUtility.ItemType.String:
                            await DatabaseManager.Current.InsertSmartPlaylistEntryAsync(id, rule.SelectedItem, rule.SelectedComparison, rule.UserInput, rule.SelectedBoolOperator);
                            break;
                        case SPUtility.ItemType.Time:
                            int seconds = 0;
                            TimeSpan ts = new TimeSpan(rule.SelectedTime.Hours, rule.SelectedTime.Minutes, seconds);
                            await DatabaseManager.Current.InsertSmartPlaylistEntryAsync(id, rule.SelectedItem, rule.SelectedComparison, ts.Ticks.ToString(), rule.SelectedBoolOperator);
                            break;
                        default:
                            break;
                    }
                }


                NavigationService.GoBack();
            }
            else
            {
                ErrorVisibility = true;
            }
        }

        private async Task LoadPlaylist()
        {
            var playlist = (SmartPlaylistItem)await DatabaseManager.Current.GetSmartPlaylistAsync(id);
            PlaylistName = playlist.Name;
            MaxSongsNumber = playlist.SongsNumber.ToString();
            SelectedSortRule = sortRules.SingleOrDefault(r => r.Option.Equals(playlist.Sorting));
            var rules = await DatabaseManager.Current.GetSmartPlaylistEntries(id);
            int counter = rules.Count;
            foreach(var rule in rules)
            {
                counter--;

                PlaylistRule pr = new PlaylistRule(resLoader);

                

                pr.SelectedItem = pr.Items.SingleOrDefault(i => i.Option.Equals(rule.Item)).Option;
                var type = SPUtility.Item.GetItemType(rule.Item);
                pr.ChangeComparisonItems();

                pr.SelectedBoolOperator = pr.BoolOperators.SingleOrDefault(b => b.Option.Equals(rule.Operator)).Option;
                pr.SelectedComparison = pr.ComparisonItems.SingleOrDefault(c => c.Option.Equals(rule.Comparison)).Option;

                if (counter > 0)
                {
                    pr.IsBoolOperatorsVisible = true;
                }

                pr.ChangeInputsVisibility();
                switch (type)
                {
                    case SPUtility.ItemType.Date:
                        pr.SelectedDate = new DateTime(Int32.Parse(rule.Value));
                        break;
                    case SPUtility.ItemType.Number:
                        pr.UserInput = rule.Value;
                        break;
                    case SPUtility.ItemType.String:
                        pr.UserInput = rule.Value;
                        break;
                    case SPUtility.ItemType.Time:
                        pr.SelectedTime = new TimeSpan(Int32.Parse(rule.Value));
                        break;
                    default:
                        break;
                }

                PlaylistRules.Add(pr);
            }
            PlaylistRules.FirstOrDefault().SelectedBoolOperator = PlaylistRules.FirstOrDefault().BoolOperators.LastOrDefault().Option;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {           
            if (parameter != null)
            {
                id = Int32.Parse(parameter.ToString());
                PlaylistRules.Clear();
                await LoadPlaylist();
            }   
            //PlaylistName = (suspensionState.ContainsKey(nameof(PlaylistName))) ? suspensionState[nameof(PlaylistName)]?.ToString() : "";
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //suspensionState[nameof(PlaylistName)] = PlaylistName;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back)
            {
                System.Diagnostics.Debug.WriteLine("NavigationMode.Back");
                PlaylistName = "";
                PlaylistRules = new ObservableCollection<PlaylistRule>();
                PlaylistRules.Add(new PlaylistRule(resLoader));
                SelectedSortRule = SortRules.FirstOrDefault();
                id = -1;
            }
            args.Cancel = false;
            await Task.CompletedTask;
        }

    }
}
