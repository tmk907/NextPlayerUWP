using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;

namespace NextPlayerUWPDataLayer.Model
{
    public class FolderItem : MusicItem, INotifyPropertyChanged
    {
        protected string folder;
        public string Folder { get { return folder; } }
        protected string directory;
        public string Directory { get { return directory; } }
        protected int songsNumber;
        public int SongsNumber
        {
            get
            {
                return songsNumber;
            }
            set
            {
                if (value != songsNumber)
                {
                    songsNumber = value;
                    onPropertyChanged(this, "SongsNumber");
                }
            }
        }
        protected DateTime lastAdded;
        public DateTime LastAdded
        {
            get { return lastAdded; }
            set
            {
                if (value != lastAdded)
                {
                    lastAdded = value;
                    onPropertyChanged(this, "LastAdded");
                }
            }
        }

        public FolderItem()
        {
            folder = "Unknown Folder";
            directory = "";
            songsNumber = 0;
            lastAdded = DateTime.MinValue;
        }

        public FolderItem(FoldersTable table)
        {
            songsNumber = table.SongsNumber;
            folder = table.Folder;
            directory = table.Directory;
            lastAdded = table.LastAdded;
        }

        public FolderItem(string folder, string directory)
        {
            this.folder = folder;
            this.directory = directory;
        }

        public override string ToString()
        {
            return "folder|" + directory;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string GetParameter()
        {
            return MusicItemTypes.folder + separator + directory;
        }
    }
}
