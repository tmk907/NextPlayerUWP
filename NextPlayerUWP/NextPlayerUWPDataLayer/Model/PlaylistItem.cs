﻿using System;
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
