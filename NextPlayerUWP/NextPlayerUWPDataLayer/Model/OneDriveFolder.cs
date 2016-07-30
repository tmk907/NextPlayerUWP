using System;

namespace NextPlayerUWPDataLayer.Model
{
    public class OneDriveFolder : FolderItem
    {
        public string Id { get; set; }
        public string ParentId { get; set; }

        public OneDriveFolder()
        {
            folder = "Unknown Folder";
            directory = "";
            songsNumber = 0;
            lastAdded = DateTime.MinValue;
            Id = "";
        }

        public OneDriveFolder(string folder, string directory, int songsNumber, string id, string parentId)
        {
            this.folder = folder;
            this.directory = directory;
            this.songsNumber = songsNumber;
            Id = id;
            ParentId = parentId;
        }

        public override string GetParameter()
        {
            return MusicItemTypes.onedrivefolder + separator + Id;
        }
    }
}
