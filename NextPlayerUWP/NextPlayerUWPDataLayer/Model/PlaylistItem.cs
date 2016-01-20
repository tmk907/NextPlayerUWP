using System;
using System.ComponentModel;

namespace NextPlayerUWPDataLayer.Model
{
    public class PlaylistItem : MusicItem, INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != name)
                {
                    name = value;
                    onPropertyChanged(this, "Name");
                }
            }
        }
        private int id;
        public int Id { get { return id; } }
        private bool isSmart;
        public bool IsSmart { get { return isSmart; } }
        private bool isNotDefault;
        public bool IsNotDefault { get { return isNotDefault; } }

        public PlaylistItem(int id, bool issmart, string name)
        {
            this.id = id;
            this.isSmart = issmart;
            this.name = name;
            if (issmart)
            {
                this.isNotDefault = !Helpers.SmartPlaylistHelper.IsDefaultSmartPlaylist(id);
            }
            else
            {
                this.isNotDefault = true;
            }
        }

        public override string ToString()
        {
            if (IsSmart) return "smart|" + id.ToString();
            else return "plain|" + id.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string GetParameter()
        {
            return "playlist" + separator + ((isSmart)?"smart":"plain") + separator + id;
        }
    }
}
