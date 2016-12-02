using System;
using System.ComponentModel;

namespace NextPlayerUWPDataLayer.Model
{
    public class PlaylistFilterElement : INotifyPropertyChanged
    {
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (value != isChecked)
                {
                    isChecked = value;
                    onPropertyChanged(this, "IsChecked");
                    action.Invoke();
                }
            }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    onPropertyChanged(this, "Name");
                }
            }
        }

        private Action action;

        public PlaylistFilterElement(Action action)
        {
            this.action = action;
        }

        public void Clicked()
        {
            IsChecked = !isChecked;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
