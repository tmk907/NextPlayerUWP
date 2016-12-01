using NextPlayerUWPDataLayer.Tables;
using System;
using System.Collections.Generic;
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
        public bool IsSmartAndNotDefault { get { return IsSmart && isNotDefault; } }
        private string path;
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (value != path)
                {
                    path = value;
                    onPropertyChanged(this, "Path");
                }
            }
        }
        private DateTime dateModified;
        public DateTime DateModified
        {
            get
            {
                return dateModified;
            }
            set
            {
                if (value != dateModified)
                {
                    dateModified = value;
                    onPropertyChanged(this, "DateModified");
                }
            }
        }
        private bool isHidden;
        public bool IsHidden
        {
            get
            {
                return isHidden;
            }
            set
            {
                if (value != isHidden)
                {
                    isHidden = value;
                    onPropertyChanged(this, "IsHidden");
                }
            }
        }

        public PlaylistItem(int id, bool issmart, string _name)
        {
            this.id = id;
            isSmart = issmart;
            if (issmart)
            {
                isNotDefault = !Helpers.SmartPlaylistHelper.IsDefaultSmartPlaylist(id);

                Dictionary<int, string> ids = Helpers.ApplicationSettingsHelper.PredefinedSmartPlaylistsId();
                if (ids.ContainsKey(id))
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    name = loader.GetString(ids[id]);
                }
                else
                {
                    name = _name;
                }
            }
            else
            {
                name = _name;
                isNotDefault = true;
            }
            path = "";
            dateModified = DateTime.MinValue;
        }

        public PlaylistItem(PlainPlaylistsTable table)
        {
            this.id = table.PlainPlaylistId;
            isSmart = false;
            this.path = table.Path ?? "";
            this.dateModified = table.DateModified;
            name = table.Name;
            isNotDefault = true;
            isHidden = table.IsHidden;
        }

        public PlaylistItem(SmartPlaylistsTable table)
        {
            this.id = table.SmartPlaylistId;
            isSmart = true;
            this.path = "";
            this.dateModified = DateTime.MinValue;
            isNotDefault = !Helpers.SmartPlaylistHelper.IsDefaultSmartPlaylist(id);
            Dictionary<int, string> ids = Helpers.ApplicationSettingsHelper.PredefinedSmartPlaylistsId();
            if (ids.ContainsKey(id))
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                name = loader.GetString(ids[id]);
            }
            else
            {
                name = table.Name;
            }
            isHidden = table.Hide;
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
            return ((isSmart)? MusicItemTypes .smartplaylist: MusicItemTypes.plainplaylist) + separator + id;
        }
    }
}
