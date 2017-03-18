using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWP.Views;
using System.Linq;
using Template10.Common;
using Windows.System;
using Windows.UI.Xaml;

namespace NextPlayerUWP.Common
{
    public class AppKeyboardShortcuts
    {
        //Ctrl+
        private VirtualKey playPauseKey = VirtualKey.Space;
        private VirtualKey nextSongKey = VirtualKey.Right;
        private VirtualKey previousSongKey = VirtualKey.Left;
        private VirtualKey changeShuffleKey = VirtualKey.N;
        private VirtualKey changeRepeatKey = VirtualKey.M;
        private VirtualKey showLyricsKey = VirtualKey.L;
        private VirtualKey showNowPlayingListKey = VirtualKey.K;
        private VirtualKey showAudioSettingsKey = VirtualKey.J;
        private VirtualKey searchKey = VirtualKey.F;


        private bool isCtrlPressed = false;
        private bool isAltPressed = false;

        private PlayerCommands playerCommands = new PlayerCommands();

        public void RegisterShortcuts()
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        public void DeregisterShortcuts()
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
        }

        private void Dispatcher_AcceleratorKeyActivated(Windows.UI.Core.CoreDispatcher sender, Windows.UI.Core.AcceleratorKeyEventArgs args)
        {
            if (args.EventType == Windows.UI.Core.CoreAcceleratorKeyEventType.KeyDown)
            {
                System.Diagnostics.Debug.WriteLine("AccKey down: {0}", args.VirtualKey);

                if (args.VirtualKey == VirtualKey.Menu)
                {
                    isAltPressed = true;
                }
                else if (args.VirtualKey == VirtualKey.Control)// if alt is pressed this is also true
                {
                    isCtrlPressed = true;
                }
                else if (isAltPressed)
                {
                    switch (args.VirtualKey)
                    {
                        case VirtualKey.Number1:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 1 });
                            break;
                        case VirtualKey.Number2:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 2 });
                            break;
                        case VirtualKey.Number3:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 3 });
                            break;
                        case VirtualKey.Number4:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 4 });
                            break;
                        case VirtualKey.Number5:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 5 });
                            break;
                        case VirtualKey.Number6:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 6 });
                            break;
                        case VirtualKey.Number7:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 7 });
                            break;
                        case VirtualKey.Number8:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 8 });
                            break;
                        case VirtualKey.Number9:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 9 });
                            break;
                        case VirtualKey.Number0:
                            MessageHub.Instance.Publish<MenuButtonSelected>(new MenuButtonSelected() { Nr = 0 });
                            break;
                    }
                }
                else if (isCtrlPressed)
                {
                    if (args.VirtualKey == previousSongKey)
                    {
                        playerCommands.Previous();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == playPauseKey)
                    {
                        playerCommands.TogglePlayPause();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == nextSongKey)
                    {
                        playerCommands.Next();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == changeShuffleKey)
                    {
                        playerCommands.ToggleShuffle();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == changeRepeatKey)
                    {
                        playerCommands.ToggleRepeat();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == showAudioSettingsKey)
                    {
                        try
                        {
                            var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
                            nav.Navigate(typeof(AudioSettingsView));
                        }
                        catch (System.Exception ex)
                        {

                        }
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == showNowPlayingListKey)
                    {
                        MessageHub.Instance.Publish<ShowNowPlayingList>(new ShowNowPlayingList());
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == showLyricsKey)
                    {
                        MessageHub.Instance.Publish<ShowLyrics>(new ShowLyrics());
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == searchKey)
                    {
                        MessageHub.Instance.Publish<EnableSearching>(new EnableSearching());
                        args.Handled = true;
                    }
                }
            }
            else if (args.EventType == Windows.UI.Core.CoreAcceleratorKeyEventType.KeyUp)
            {
                System.Diagnostics.Debug.WriteLine("AccKey up {0}", args.VirtualKey);
                if (args.VirtualKey == VirtualKey.Control)
                {
                    isCtrlPressed = false;
                    isAltPressed = false;
                }
                else if (args.VirtualKey == VirtualKey.Menu)
                {
                    isAltPressed = false;
                    isCtrlPressed = false;
                }
            }
        }
    }
}
