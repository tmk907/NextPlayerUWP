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
using Windows.UI.Xaml.Controls;

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
            selectedBoolOperator = BoolOperators.FirstOrDefault().Option;
        }

        #region Properties

        public List<ComboBoxItemValue> Items { get; }
        private string selectedItem;
        public string SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != selectedItem && value != null)
                {
                    switch (SPUtility.Item.GetItemType(value))
                    {
                        case SPUtility.ItemType.Date:
                            IsDatePickerVisible = true;
                            IsTimePickerVisible = false;
                            IsTextBoxVisible = false;
                            ChangeComparisonItems(SPUtility.ItemType.Date);
                            break;
                        case SPUtility.ItemType.Number:
                            IsDatePickerVisible = false;
                            IsTimePickerVisible = false;
                            IsTextBoxVisible = true;
                            ChangeComparisonItems(SPUtility.ItemType.Number);
                            break;
                        case SPUtility.ItemType.String:
                            IsDatePickerVisible = false;
                            IsTimePickerVisible = false;
                            IsTextBoxVisible = true;
                            ChangeComparisonItems(SPUtility.ItemType.String);
                            break;
                        case SPUtility.ItemType.Time:
                            IsDatePickerVisible = false;
                            IsTimePickerVisible = true;
                            IsTextBoxVisible = false;
                            ChangeComparisonItems(SPUtility.ItemType.Time);
                            break;

                        default:
                            break;
                    }
                }
                selectedItem = value;
                //Set(ref selectedItem, value);
            }
        }

        private ObservableCollection<ComboBoxItemValue> comparisonItems = new ObservableCollection<ComboBoxItemValue>();
        public ObservableCollection<ComboBoxItemValue> ComparisonItems
        {
            get { return comparisonItems; }
            set { Set(ref comparisonItems, value); }
        }

        private string selectedComparison;
        public string SelectedComparison
        {
            get { return selectedComparison; }
            set
            {
                selectedComparison = value;
                //Set(ref selectedComparison, value);
            }
        }

        private string userInput = "";
        public string UserInput
        {
            get { return userInput; }
            set { Set(ref userInput, value); }
        }

        private ObservableCollection<ComboBoxItemValue> boolOperators;
        public ObservableCollection<ComboBoxItemValue> BoolOperators
        {
            get { return boolOperators; }
            set { Set(ref boolOperators, value); }
        }

        private string selectedBoolOperator;
        public string SelectedBoolOperator
        {
            get { return selectedBoolOperator; }
            set
            {
                selectedBoolOperator = value;
                //Set(ref selectedBoolOperator, value);
            }
        }

        private bool isBoolOperatorsVisible = false;
        public bool IsBoolOperatorsVisible
        {
            get { return isBoolOperatorsVisible; }
            set { Set(ref isBoolOperatorsVisible, value); }
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

        public void ChangeInputsVisibility()
        {
            ChangeInputsVisibility(SPUtility.Item.GetItemType(SelectedItem));
        }

        private void ChangeInputsVisibility(SPUtility.ItemType type)
        {
            switch (type)
            {
                case SPUtility.ItemType.Date:
                    IsDatePickerVisible = true;
                    IsTimePickerVisible = false;
                    IsTextBoxVisible = false;
                    break;
                case SPUtility.ItemType.Number:
                    IsDatePickerVisible = false;
                    IsTimePickerVisible = false;
                    IsTextBoxVisible = true;
                    break;
                case SPUtility.ItemType.String:
                    IsDatePickerVisible = false;
                    IsTimePickerVisible = false;
                    IsTextBoxVisible = true;
                    break;
                case SPUtility.ItemType.Time:
                    IsDatePickerVisible = false;
                    IsTimePickerVisible = true;
                    IsTextBoxVisible = false;
                    break;
                default:
                    break;
            }
        }

        public void ChangeComparisonItems()
        {
            ChangeComparisonItems(SPUtility.Item.GetItemType(SelectedItem));
        }

        private void ChangeComparisonItems(SPUtility.ItemType type)
        {
            ComparisonItems.Clear();
            switch (type)
            {
                case SPUtility.ItemType.Date:
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsLess, loader.GetString(SPUtility.Comparison.IsLess)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsGreater, loader.GetString(SPUtility.Comparison.IsGreater)));
                    break;
                case SPUtility.ItemType.Number:
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.Is, loader.GetString(SPUtility.Comparison.Is)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsNot, loader.GetString(SPUtility.Comparison.IsNot)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsLess, loader.GetString(SPUtility.Comparison.IsLess)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsGreater, loader.GetString(SPUtility.Comparison.IsGreater)));
                    break;
                case SPUtility.ItemType.String:
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.Contains, loader.GetString(SPUtility.Comparison.Contains)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.DoesNotContain, loader.GetString(SPUtility.Comparison.DoesNotContain)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.StartsWith, loader.GetString(SPUtility.Comparison.StartsWith)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.EndsWith, loader.GetString(SPUtility.Comparison.EndsWith)));
                    break;
                case SPUtility.ItemType.Time:
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsLess, loader.GetString(SPUtility.Comparison.IsLess)));
                    ComparisonItems.Add(new ComboBoxItemValue(SPUtility.Comparison.IsGreater, loader.GetString(SPUtility.Comparison.IsGreater)));
                    break;
                default:
                    break;
            }
            SelectedComparison = ComparisonItems.FirstOrDefault().Option;
        }

        public void SelectSort()
        {
            SelectedComparison = comparisonItems.FirstOrDefault().Option;
        }

        public void SelectBool()
        {
            SelectedBoolOperator = BoolOperators.FirstOrDefault().Option;
        }

        public void InstanceBool()
        {
            BoolOperators = new ObservableCollection<ComboBoxItemValue>();
            BoolOperators.Add(new ComboBoxItemValue(SPUtility.Operator.And, loader.GetString(SPUtility.Operator.And)));
            //BoolOperators.Add(new ComboBoxItemValue(SPUtility.Operator.Or, loader.GetString(SPUtility.Operator.Or)));
        }

        public bool IsCorrect()
        {
            bool isCorrect = true;

            if (SPUtility.Item.IsNumberType(selectedItem))
            {
                int number;
                if (userInput == "" || !Int32.TryParse(userInput, out number))
                {
                    isCorrect = false;
                }
            }
            else if (SPUtility.Item.IsDateType(selectedItem))
            {
                
            }
            else if (SPUtility.Item.IsTimeType(selectedItem))
            {
                
            }
            else
            {
                if (userInput == "")
                {
                    isCorrect = false;
                }
            }
            if (selectedComparison == null)
            {
                isCorrect = false;
            }
            if (selectedBoolOperator == null)
            {
                isCorrect = false;
            }
            if (selectedItem == null)
            {
                isCorrect = false;
            }

            return isCorrect;
        }

    }
}
