using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWP.Helpers
{
    public class PlaylistRule : Template10.Mvvm.BindableBase
    {
        private ResourceLoader loader;
        public PlaylistRule(ResourceLoader resLoader)
        {
            loader = resLoader;
            Items = new List<ComboBoxItemValue>();
            foreach (var k in typeof(SPUtility.Item).GetRuntimeFields())
            {
                object o = k.GetValue(null);
                if (o is string)
                {
                    Items.Add(new ComboBoxItemValue(o as string, loader.GetString(o as string)));
                }
            }
            //selectedItem = Items.FirstOrDefault();

            BoolOperators = new ObservableCollection<ComboBoxItemValue>();
            BoolOperators.Add(new ComboBoxItemValue(SPUtility.Operator.And, loader.GetString(SPUtility.Operator.And)));
            BoolOperators.Add(new ComboBoxItemValue(SPUtility.Operator.Or, loader.GetString(SPUtility.Operator.Or)));
            //selectedBoolOperator = BoolOperators.FirstOrDefault();
        }

        #region Properties

        public List<ComboBoxItemValue> Items { get; }

        public ComboBoxItemValue selectedItem;
        public ComboBoxItemValue SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != selectedItem)
                {
                    if (SPUtility.Item.IsNumberType(value.Option))
                    {
                        IsDatePickerVisible = false;
                        IsTimePickerVisible = false;
                        IsTextBoxVisible = true;
                        ChangeComparisonItems(false);
                    }
                    else if (SPUtility.Item.IsDateType(value.Option))
                    {
                        IsDatePickerVisible = true;
                        IsTimePickerVisible = false;
                        IsTextBoxVisible = false;
                        ChangeComparisonItems(false);
                    }
                    else if (SPUtility.Item.IsTimeType(value.Option))
                    {
                        IsDatePickerVisible = false;
                        IsTimePickerVisible = true;
                        IsTextBoxVisible = false;
                        ChangeComparisonItems(false);
                    }
                    else
                    {
                        IsDatePickerVisible = false;
                        IsTimePickerVisible = false;
                        IsTextBoxVisible = true;
                        ChangeComparisonItems(true);
                    }
                }
                Set(ref selectedItem, value);
            }
        }

        private ObservableCollection<ComboBoxItemValue> comparisonItems = new ObservableCollection<ComboBoxItemValue>();
        public ObservableCollection<ComboBoxItemValue> ComparisonItems
        {
            get { return comparisonItems; }
            set { Set(ref comparisonItems, value); }
        }

        public ComboBoxItemValue selectedComparison;
        public ComboBoxItemValue SelectedComparison
        {
            get { return selectedComparison; }
            set { Set(ref selectedComparison, value); }
        }

        public string userInput = "";
        public string UserInput
        {
            get { return userInput; }
            set { Set(ref userInput, value); }
        }

        public ObservableCollection<ComboBoxItemValue> boolOperators;
        public ObservableCollection<ComboBoxItemValue> BoolOperators
        {
            get { return boolOperators; }
            set { Set(ref boolOperators, value); }
        }

        public ComboBoxItemValue selectedBoolOperator;
        public ComboBoxItemValue SelectedBoolOperator
        {
            get { return selectedBoolOperator; }
            set { Set(ref selectedBoolOperator, value); }
        }

        private bool isDatePickerVisible = false;
        public bool IsDatePickerVisible
        {
            get { return isDatePickerVisible; }
            set { Set(ref isDatePickerVisible, value); }
        }

        private DateTime selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set { Set(ref selectedDate, value); }
        }

        private bool isTimePickerVisible = false;
        public bool IsTimePickerVisible
        {
            get { return isTimePickerVisible; }
            set { Set(ref isTimePickerVisible, value); }
        }

        private TimeSpan selectedTime = TimeSpan.Zero;
        public TimeSpan SelectedTime
        {
            get { return selectedTime; }
            set { Set(ref selectedTime, value); }
        }

        private bool isTextBoxVisible = false;
        public bool IsTextBoxVisible
        {
            get { return isTextBoxVisible; }
            set { Set(ref isTextBoxVisible, value); }
        }

        #endregion

        private void ChangeComparisonItems(bool isStringCompare)
        {
            ComparisonItems.Clear();
            if (isStringCompare)
            {
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.Contains, loader.GetString(SPUtility.Comparison.Contains)));
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.DoesNotContain, loader.GetString(SPUtility.Comparison.DoesNotContain)));
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.StartsWith, loader.GetString(SPUtility.Comparison.StartsWith)));
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.EndsWith, loader.GetString(SPUtility.Comparison.EndsWith)));
            }
            else
            {
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.Is, loader.GetString(SPUtility.Comparison.Is)));
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsNot, loader.GetString(SPUtility.Comparison.IsNot)));
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsLess, loader.GetString(SPUtility.Comparison.IsLess)));
                comparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsGreater, loader.GetString(SPUtility.Comparison.IsGreater)));
            }
            SelectedComparison = comparisonItems.FirstOrDefault();
        }

        public bool IsCorrect()
        {
            bool isCorrect = true;

            if (SPUtility.Item.IsNumberType(selectedItem.Option))
            {
                int number;
                if (userInput == "" || !Int32.TryParse(userInput, out number))
                {
                    isCorrect = false;
                }
            }
            else if (SPUtility.Item.IsDateType(selectedItem.Option))
            {
                
            }
            else if (SPUtility.Item.IsTimeType(selectedItem.Option))
            {
                
            }
            else
            {
                if (userInput == "")
                {
                    isCorrect = false;
                }
            }

            return isCorrect;
        }

    }
}
