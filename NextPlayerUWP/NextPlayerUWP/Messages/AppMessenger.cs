using GalaSoft.MvvmLight.Messaging;

namespace NextPlayerUWP.Messages
{
    public sealed class AppMessenger
    {
        public static void ShowInAppNotification(string text1, string text2 = null)
        {
            InAppNotification message = new InAppNotification() { FirstTextLine = text1, SecondTextLine = text2 };
            Send<InAppNotification>(message, MessageNames.InAppNotification);
        }

        public static void ChangeTheme(bool isLightTheme)
        {
            ThemeChange message = new ThemeChange() { IsLightTheme = isLightTheme };
            Send<ThemeChange>(message, MessageNames.ThemeChange);
        }

        public static void MenuButtonSelected(int nr)
        {
            MenuButtonSelected message = new MenuButtonSelected()
            {
                Nr = nr
            };
            Send<MenuButtonSelected>(message, MessageNames.MenuButtonSelected);
        }

        public static void KeyboardShortcutPressed(bool isCtrl, bool isAlt, string key)
        {
            KeyboardShortcut message = new KeyboardShortcut()
            {
                IsAlt = isAlt,
                IsCtrl = isCtrl,
                Key = key,
            };
            Send<KeyboardShortcut>(message, MessageNames.KeyboardShortcut);
        }

        public static void ShowLyrics()
        {
            Send<ShowLyrics>(new ShowLyrics(), MessageNames.ShowLyrics);
        }

        public static void ShowNowPlayingList()
        {
            Send<ShowNowPlayingList>(new ShowNowPlayingList(), MessageNames.ShowNowPlayingList);
        }

        public static void EnableSearching()
        {
            Send<EnableSearching>(new EnableSearching(), MessageNames.EnableSerching);
        }

        private static void Send<T>(T message, string name)
        {
            Messenger.Default.Send(new NotificationMessage<T>(message, name));
        }
    }
}
