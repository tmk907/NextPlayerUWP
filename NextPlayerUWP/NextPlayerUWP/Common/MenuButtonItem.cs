namespace NextPlayerUWP.Common
{
    public class MenuButtonItem : Template10.Mvvm.BindableBase
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
            set { Set(ref name, value); }
        }

        private bool showButton;
        public bool ShowButton
        {
            get { return showButton; }
            set { Set(ref showButton, value); }
        }
    }
}
