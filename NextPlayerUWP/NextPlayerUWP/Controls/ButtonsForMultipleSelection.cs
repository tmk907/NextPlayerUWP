using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Linq;
using Template10.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Controls
{
    public class ButtonsForMultipleSelection
    {
        private MusicViewModelBase ViewModel;
        private PageHeader PageHeader;
        private IGetSelectedItems ListView;
        private bool isLoaded = false;
        private string ButtonForMultipleSelection = "ButtonForMultipleSelection";

        public void OnLoaded(MusicViewModelBase viewModel, PageHeader pageHeader, IGetSelectedItems listView)
        {
            ViewModel = viewModel;
            PageHeader = pageHeader;
            ListView = listView;
            isLoaded = true;
        }

        public void OnUnloaded()
        {
            HideMultipleSelectionButtons();
            isLoaded = false;
            ViewModel = null;
            ListView = null;
            PageHeader = null;
        }

        public void ShowMultipleSelectionButtons()
        {
            if (!isLoaded) return;
            var list = GetButtonsForMultipleSelection();
            if (DeviceFamilyHelper.IsMobile())
            {
                foreach (var item in list)
                {
                    PageHeader.SecondaryCommands.Add(item);
                }
            }
            else
            {
                PageHeader.PrimaryCommands.Add(new AppBarSeparator() { Tag = ButtonForMultipleSelection });
                foreach (var item in list)
                {
                    PageHeader.PrimaryCommands.Add(item);
                }
            }
        }

        public void HideMultipleSelectionButtons()
        {
            if (!isLoaded) return;
            if (DeviceFamilyHelper.IsMobile())
            {
                for (int i = 1; i <= 5; i++)
                {
                    var buttons = PageHeader.SecondaryCommands.OfType<Control>().Where(a => (a?.Tag as string) != ButtonForMultipleSelection).ToList();
                    PageHeader.SecondaryCommands.Clear();
                    foreach(ICommandBarElement item in buttons)
                    {
                        PageHeader.SecondaryCommands.Add(item);
                    }
                }
            }
            else
            {
                var buttons = PageHeader.PrimaryCommands.OfType<Control>().Where(a => (a?.Tag as string) != ButtonForMultipleSelection).ToList();
                PageHeader.PrimaryCommands.Clear();
                foreach (ICommandBarElement item in buttons)
                {
                    PageHeader.PrimaryCommands.Add(item);
                }
            }
        }

        private List<ICommandBarElement> GetButtonsForMultipleSelection()
        {
            TranslationHelper tr = new TranslationHelper();
            List<ICommandBarElement> list = new List<ICommandBarElement>();
            AppBarButton buttonPlay = new AppBarButton()
            {
                Label = tr.GetTranslation("AppBarButtonPlayNow/Label"),
                Icon = new SymbolIcon(Symbol.Play),
                Tag = ButtonForMultipleSelection
            };
            buttonPlay.Click += PlayNowMultiple;
            AppBarButton buttonPlayNext = new AppBarButton()
            {
                Label = tr.GetTranslation("AppBarButtonPlayNext/Label"),
                Icon = new SymbolIcon(Symbol.Next),
                Tag = ButtonForMultipleSelection
            };
            buttonPlayNext.Click += PlayNextMultiple;
            AppBarButton buttonAddToNowPlaying = new AppBarButton()
            {
                Label = tr.GetTranslation("AppBarButtonAddToNowPlaying/Label"),
                Icon = new SymbolIcon(Symbol.Add),
                Tag = ButtonForMultipleSelection
            };
            buttonAddToNowPlaying.Click += AddToNowPlayingMultiple;
            AppBarButton buttonAddToPlaylist = new AppBarButton()
            {
                Label = tr.GetTranslation("AppBarButtonAddToPlaylist/Label"),
                Icon = new SymbolIcon(Symbol.Add),
                Tag = ButtonForMultipleSelection
            };
            buttonAddToPlaylist.Click += AddToPlaylistMultiple;

            list.Add(buttonPlay);
            list.Add(buttonPlayNext);
            list.Add(buttonAddToNowPlaying);
            list.Add(new AppBarSeparator() { Tag = ButtonForMultipleSelection });
            list.Add(buttonAddToPlaylist);
            return list;
        }

        private async void PlayNowMultiple(object sender, RoutedEventArgs e)
        {
            HideMultipleSelectionButtons();
            var items = ListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) await ViewModel.PlayNowMany(items);
        }

        private async void PlayNextMultiple(object sender, RoutedEventArgs e)
        {
            HideMultipleSelectionButtons();
            var items = ListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) await ViewModel.PlayNextMany(items);
        }

        private async void AddToNowPlayingMultiple(object sender, RoutedEventArgs e)
        {
            HideMultipleSelectionButtons();
            var items = ListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) await ViewModel.AddToNowPlayingMany(items);
        }

        private void AddToPlaylistMultiple(object sender, RoutedEventArgs e)
        {
            HideMultipleSelectionButtons();
            var items = ListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) ViewModel.AddToPlaylistMany(items);
        }
    }
}
