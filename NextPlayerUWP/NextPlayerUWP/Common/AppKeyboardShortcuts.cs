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

        public void RegisterShortcuts()
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
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
                            App.Highlight(1);
                            break;
                        case VirtualKey.Number2:
                            App.Highlight(2);
                            break;
                        case VirtualKey.Number3:
                            App.Highlight(3);
                            break;
                        case VirtualKey.Number4:
                            App.Highlight(4);
                            break;
                        case VirtualKey.Number5:
                            App.Highlight(5);
                            break;
                        case VirtualKey.Number6:
                            App.Highlight(6);
                            break;
                        case VirtualKey.Number7:
                            App.Highlight(7);
                            break;
                        case VirtualKey.Number8:
                            App.Highlight(8);
                            break;
                        case VirtualKey.Number0:
                            App.Highlight(0);
                            break;
                    }
                }
                else if (isCtrlPressed)
                {
                    if (args.VirtualKey == previousSongKey)
                    {
                        PlaybackService.Instance.Previous();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == playPauseKey)
                    {
                        PlaybackService.Instance.TogglePlayPause();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == nextSongKey)
                    {
                        PlaybackService.Instance.Next();
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == changeShuffleKey)
                    {
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == changeRepeatKey)
                    {
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == showAudioSettingsKey)
                    {
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == showNowPlayingListKey)
                    {
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == showLyricsKey)
                    {
                        args.Handled = true;
                    }
                    else if (args.VirtualKey == searchKey)
                    {
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

        public void DeregisterShortcuts()
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
        }
    }
}
