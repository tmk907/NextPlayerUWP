using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NextPlayerWinUI.Views;
using NextPlayerWinUIDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NextPlayerWinUI.Common
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

    /// <summary>What selecting a menu entry does. Stored in <see cref="NavigationViewItem"/>'s Tag.</summary>
    public sealed record MenuDestination(Type PageType, bool ClearHistory);

    public class HamburgerMenuBuilder
    {
        private static readonly Dictionary<MenuItemType, string> TextKeys = new()
        {
            { MenuItemType.Albums, "TBAlbums/Text" },
            { MenuItemType.AlbumArtists, "TBAlbumArtists/Text" },
            { MenuItemType.Artists, "TBArtists/Text" },
            { MenuItemType.Folders, "TBFolders/Text" },
            { MenuItemType.Genres, "TBGenres/Text" },
            { MenuItemType.NowPlayingSong, "TBNowPlaying/Text" },
            { MenuItemType.NowPlayingPlaylist, "TBNowPlaying/Text" },
            { MenuItemType.Playlists, "TBPlaylists/Text" },
            { MenuItemType.Songs, "TBSongs/Text" },
            { MenuItemType.Online, "TBOnline/Text" },
        };

        private static readonly Dictionary<MenuItemType, Type> PageTypes = new()
        {
            { MenuItemType.Albums, typeof(AlbumsView) },
            { MenuItemType.AlbumArtists, typeof(AlbumArtistsView) },
            { MenuItemType.Artists, typeof(ArtistsView) },
            { MenuItemType.Folders, typeof(FoldersRootView) },
            { MenuItemType.Genres, typeof(GenresView) },
            { MenuItemType.NowPlayingSong, typeof(NowPlayingDesktopView) },
            { MenuItemType.NowPlayingPlaylist, typeof(NowPlayingView) },
            { MenuItemType.Playlists, typeof(PlaylistsView) },
            { MenuItemType.Online, typeof(RadiosView) },
            { MenuItemType.Songs, typeof(SongsView) },
        };

        private readonly TranslationHelper helper = new TranslationHelper();

        public string GetMenuItemText(MenuItemType type)
            => TextKeys.TryGetValue(type, out var key) ? helper.GetTranslation(key) : "";

        public ObservableCollection<NavigationViewItem> GetButtons()
        {
            var list = ApplicationSettingsHelper.ReadData<List<MenuButtonItem>>(SettingsKeys.MenuEntries);

            // Desktop has a single "Now playing" entry, which older settings may have stored as the mobile one.
            var mobileNowPlaying = list.First(b => b.PageType == MenuItemType.NowPlayingPlaylist);
            if (mobileNowPlaying.ShowButton)
            {
                mobileNowPlaying.ShowButton = false;
                list.First(b => b.PageType == MenuItemType.NowPlayingSong).ShowButton = true;
            }

            return new ObservableCollection<NavigationViewItem>(list.Where(b => b.ShowButton).Select(b => MakeButton(b.PageType)));
        }

        public NavigationViewItem MakeSettingsButton()
        {
            var item = new NavigationViewItem
            {
                Content = helper.GetTranslation("TBSettings/Text"),
                Icon = new SymbolIcon(Symbol.Setting),
                Tag = new MenuDestination(typeof(SettingsView2), ClearHistory: false),
            };
            AutomationProperties.SetAutomationId(item, "MenuSettings");
            return item;
        }

        private NavigationViewItem MakeButton(MenuItemType page)
        {
            bool clearHistory = page != MenuItemType.Online && page != MenuItemType.NowPlayingSong && page != MenuItemType.NowPlayingPlaylist;
            var item = new NavigationViewItem
            {
                Content = GetMenuItemText(page),
                Icon = MakeIcon(page),
                Tag = new MenuDestination(PageTypes[page], clearHistory),
            };
            AutomationProperties.SetAutomationId(item, "Menu" + page);
            return item;
        }

        private static IconElement MakeIcon(MenuItemType page) => page switch
        {
            MenuItemType.Albums => new FontIcon { Glyph = "\xE958", FontFamily = new FontFamily("Segoe MDL2 Assets") },
            MenuItemType.AlbumArtists => new SymbolIcon(Symbol.Contact2),
            MenuItemType.Artists => new SymbolIcon(Symbol.People),
            MenuItemType.Folders => new SymbolIcon(Symbol.Copy),
            MenuItemType.Genres => new SymbolIcon(Symbol.World),
            MenuItemType.Playlists => new SymbolIcon(Symbol.MusicInfo),
            MenuItemType.Songs => new SymbolIcon(Symbol.Audio),
            MenuItemType.Online => new SymbolIcon(Symbol.View),
            _ => new SymbolIcon(Symbol.Play),
        };
    }
}
