using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;

namespace NextPlayerUWPDataLayer.Model
{
    public class FolderItem : MusicItem, INotifyPropertyChanged
    {
        private string folder;
        public string Folder { get { return folder; } }
        private string directory;
        public string Directory { get { return directory; } }
        private int songsNumber;
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
        public FolderItem()
        {
            this.folder = "Unknown Folder";
            this.songsNumber = 0;
        }

        public FolderItem(string folder, string directory, int songsnumber)
        {
            this.folder = folder;
            this.directory = directory;
            this.songsNumber = songsnumber;
        }

        public FolderItem(FoldersTable table)
        {
            songsNumber = table.SongsNumber;
            folder = table.Folder;
            directory = table.Directory;
        }

        public override string ToString()
        {
            return "folder|" + directory;
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
            return MusicItemTypes.folder + separator + directory;
        }
    }
}
