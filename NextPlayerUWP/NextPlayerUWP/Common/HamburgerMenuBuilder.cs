using NextPlayerUWP.Views;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Common
{
    public enum MenuItemType
    {
        Albums = 1,
        AlbumArtists = 2,
        Artists = 3,
        Folders = 4,
        Genres = 5,
        NowPlayingSong = 6,
        NowPlayingPlaylist = 7,
        Playlists = 8,
        Songs = 9,
        Online = 10,
    }

    public class HamburgerMenuBuilder
    {
        public HamburgerMenuBuilder()
        {
            helper = new TranslationHelper();
            names = new Dictionary<MenuItemType, string>();
        }

        private TranslationHelper helper;

        private Dictionary<MenuItemType, string> names;
        private Dictionary<MenuItemType, string> Names
        {
            get
            {
                if (names.Count == 0)
                {
                    names = TranslateButtonNames();
                }
                return names;
            }
        }
        private Dictionary<MenuItemType, string> TranslateButtonNames()
        {
            var list = ApplicationSettingsHelper.ReadData<List<MenuButtonItem>>(SettingsKeys.MenuEntries);
            Dictionary<MenuItemType, string> translations = new Dictionary<MenuItemType, string>();
            foreach (var item in list)
            {
                string name = "";
                switch (item.PageType)
                {
                    case MenuItemType.Albums:
                        name = helper.GetTranslation("TBAlbums/Text");
                        break;
                    case MenuItemType.AlbumArtists:
                        name = helper.GetTranslation("TBAlbumArtists/Text");
                        break;
                    case MenuItemType.Artists:
                        name = helper.GetTranslation("TBArtists/Text");
                        break;
                    case MenuItemType.Folders:
                        name = helper.GetTranslation("TBFolders/Text");
                        break;
                    case MenuItemType.Genres:
                        name = helper.GetTranslation("TBGenres/Text");
                        break;
                    case MenuItemType.NowPlayingSong:
                        name = helper.GetTranslation("TBNowPlaying/Text");
                        break;
                    case MenuItemType.NowPlayingPlaylist:
                        name = helper.GetTranslation("TBNowPlaying/Text");
                        break;
                    case MenuItemType.Playlists:
                        name = helper.GetTranslation("TBPlaylists/Text");
                        break;
                    case MenuItemType.Songs:
                        name = helper.GetTranslation("TBSongs/Text");
                        break;
                    case MenuItemType.Online:
                        name = helper.GetTranslation("TBOnline/Text");
                        break;
                    default:
                        break;
                }
                translations.Add(item.PageType, name);
            }
            return translations;
        }

        public string GetMenuItemText(MenuItemType type)
        {
            string name;
            Names.TryGetValue(type, out name);
            return name ?? "";
        }

        public ObservableCollection<HamburgerButtonInfo> GetButtons()
        {
            ObservableCollection<HamburgerButtonInfo> buttons = new ObservableCollection<HamburgerButtonInfo>();

            var list = ApplicationSettingsHelper.ReadData<List<MenuButtonItem>>(SettingsKeys.MenuEntries);

            if (DeviceFamilyHelper.IsDesktop())
            {
                var a = list.First(b => b.PageType == MenuItemType.NowPlayingPlaylist);
                if (a.ShowButton)
                {
                    a.ShowButton = false;
                    list.First(b => b.PageType == MenuItemType.NowPlayingSong).ShowButton = true;
                }
            }
            else
            {
                var a = list.First(b => b.PageType == MenuItemType.NowPlayingSong);
                if (a.ShowButton)
                {
                    a.ShowButton = false;
                    list.First(b => b.PageType == MenuItemType.NowPlayingPlaylist).ShowButton = true;
                }
            }

            foreach(var item in list.Where(b => b.ShowButton))
            {
                var button = MakeButton(item.PageType);
                buttons.Add(button);
            }

            return buttons;
        }

        public HamburgerButtonInfo MakeButton(MenuItemType page)
        {
            bool clearHistory = true;
            if (page == MenuItemType.Online || page == MenuItemType.NowPlayingSong || page == MenuItemType.NowPlayingPlaylist)
            {
                clearHistory = false;
            }

            var content = ButtonContent(page);
            Type pageType = PageTypes[page];
            HamburgerButtonInfo button = new HamburgerButtonInfo()
            {
                ButtonType = HamburgerButtonInfo.ButtonTypes.Toggle,
                ClearHistory = clearHistory,
                Content = content,
                PageType = pageType,
            };
            return button;
        }
        
        private UIElement ButtonContent(MenuItemType page)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
            };
            
            switch (page)
            {
                case MenuItemType.Albums:
                    var viewBox = new Viewbox()
                    {
                        Height = 20,
                        Child = new TextBlock()
                        {
                            Text = "\xE958",
                            FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                        }
                    };
                    var grid = new Grid()
                    {
                        Height = 48,
                        Width = 48,
                    };
                    grid.Children.Add(viewBox);
                    stackPanel.Children.Add(grid);
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.Albums),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.AlbumArtists:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.Contact2,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.AlbumArtists),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.Artists:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.People,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.Artists),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.Folders:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.Copy,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.Folders),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.Genres:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.World,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.Genres),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.NowPlayingSong:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.Play,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.NowPlayingSong),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.NowPlayingPlaylist:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.Play,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.NowPlayingPlaylist),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.Playlists:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.MusicInfo,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.Playlists),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.Songs:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.Audio,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.Songs),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                case MenuItemType.Online:
                    stackPanel.Children.Add(new SymbolIcon()
                    {
                        Height = 48,
                        Symbol = Symbol.View,
                        Width = 48,
                    });
                    stackPanel.Children.Add(new TextBlock()
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Text = GetMenuItemText(MenuItemType.Online),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    break;
                default:
                    break;
            }

            return stackPanel;
        }

        private readonly Dictionary<MenuItemType, Type> PageTypes = new Dictionary<MenuItemType, Type>()
        {
            { MenuItemType.Albums, typeof(AlbumsView)},
            { MenuItemType.AlbumArtists, typeof(AlbumArtistsView)},
            { MenuItemType.Artists, typeof(ArtistsView)},
            { MenuItemType.Folders, typeof(FoldersRootView)},
            { MenuItemType.Genres, typeof(GenresView)},
            //{ MenuItemType.NowPlaying, typeof(NowPlayingView)},
            { MenuItemType.NowPlayingSong, typeof(NowPlayingDesktopView)},
            { MenuItemType.NowPlayingPlaylist, typeof(NowPlayingView)},
            { MenuItemType.Playlists, typeof(PlaylistsView)},
            { MenuItemType.Online, typeof(RadiosView)},
            //{ MenuItemType.Settings, typeof(SettingsView)},
            { MenuItemType.Songs, typeof(SongsView)},
        };
    }
}
