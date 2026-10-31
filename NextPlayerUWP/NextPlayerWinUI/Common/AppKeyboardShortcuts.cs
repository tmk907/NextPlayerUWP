using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using NextPlayerWinUI.Messages;
using NextPlayerWinUI.Messages.Hub;
using NextPlayerWinUI.Views;
using Windows.System;
using Windows.UI.Core;

namespace NextPlayerWinUI.Common
{
    public class AppKeyboardShortcuts
    {
        //Ctrl+
        private const VirtualKey PlayPauseKey = VirtualKey.Space;
        private const VirtualKey NextSongKey = VirtualKey.Right;
        private const VirtualKey PreviousSongKey = VirtualKey.Left;
        private const VirtualKey ChangeShuffleKey = VirtualKey.N;
        private const VirtualKey ChangeRepeatKey = VirtualKey.M;
        private const VirtualKey ShowLyricsKey = VirtualKey.L;
        private const VirtualKey ShowNowPlayingListKey = VirtualKey.K;
        private const VirtualKey ShowAudioSettingsKey = VirtualKey.J;
        private const VirtualKey SearchKey = VirtualKey.F;

        private readonly PlayerCommands playerCommands = new PlayerCommands();

        public void RegisterShortcuts(UIElement root)
        {
            root.PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs args)
        {
            // Alt+1..9 select a menu entry, Alt+0 opens settings.
            if (IsDown(VirtualKey.Menu))
            {
                if (args.Key >= VirtualKey.Number0 && args.Key <= VirtualKey.Number9)
                {
                    MessageHub.Instance.Publish(new MenuButtonSelected() { Nr = args.Key - VirtualKey.Number0 });
                    args.Handled = true;
                }
            }
            else if (IsDown(VirtualKey.Control))
            {
                args.Handled = HandleControlShortcut(args.Key);
            }
        }

        private bool HandleControlShortcut(VirtualKey key)
        {
            switch (key)
            {
                case PreviousSongKey:
                    playerCommands.Previous();
                    return true;
                case PlayPauseKey:
                    playerCommands.TogglePlayPause();
                    return true;
                case NextSongKey:
                    playerCommands.Next();
                    return true;
                case ChangeShuffleKey:
                    playerCommands.ToggleShuffle();
                    return true;
                case ChangeRepeatKey:
                    playerCommands.ToggleRepeat();
                    return true;
                case ShowAudioSettingsKey:
                    App.NavigationService.Navigate(typeof(AudioSettingsView));
                    return true;
                case ShowNowPlayingListKey:
                    MessageHub.Instance.Publish(new ShowNowPlayingList());
                    return true;
                case ShowLyricsKey:
                    MessageHub.Instance.Publish(new ShowLyrics());
                    return true;
                case SearchKey:
                    MessageHub.Instance.Publish(new EnableSearching());
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsDown(VirtualKey key)
            => InputKeyboardSource.GetKeyStateForCurrentThread(key).HasFlag(CoreVirtualKeyStates.Down);
    }
}
