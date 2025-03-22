//using System;
//using Microsoft.UI.Xaml.Data;
//using Microsoft.UI.Xaml.Media;

//namespace NextPlayerWinUI.Converters
//{
//    public class BoolToHighlightedItemForeground : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, string language)
//        {
//            if ((bool)value)
//            {
//                return new SolidColorBrush(Microsoft.UI.Colors.White);
//            }
//            else
//            {
//                if ((string)parameter == "1")
//                {
//                    if (App.IsLightThemeOn)
//                    {
//                        var dict = App.Current.Resources.ThemeDictionaries["Light"] as Microsoft.UI.Xaml.ResourceDictionary;
//                        return dict["ListItemForegroundSolid"];
//                    }
//                    else
//                    {
//                        var dict = App.Current.Resources.ThemeDictionaries["Dark"] as Microsoft.UI.Xaml.ResourceDictionary;
//                        return dict["ListItemForegroundSolid"];
//                    }
//                }
//                else
//                {
//                    if (App.IsLightThemeOn)
//                    {
//                        var dict = App.Current.Resources.ThemeDictionaries["Light"] as Microsoft.UI.Xaml.ResourceDictionary;
//                        return dict["ListItemForegroundLight"];
//                    }
//                    else
//                    {
//                        var dict = App.Current.Resources.ThemeDictionaries["Dark"] as Microsoft.UI.Xaml.ResourceDictionary;
//                        return dict["ListItemForegroundLight"];
//                    }
//                }
//            }
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, string language)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
