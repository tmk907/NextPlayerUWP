using CommunityToolkit.Mvvm.ComponentModel;

namespace NextPlayerWinUI.Common
{
    public class MenuButtonItem : ObservableObject
    {
        public MenuButtonItem()
        {
            name = "";
            PageType = MenuItemType.Playlists;
            showButton = false;
        }

        public MenuItemType PageType { get; set; }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private bool showButton;
        public bool ShowButton
        {
            get { return showButton; }
            set { SetProperty(ref showButton, value); }
        }
    }
}
