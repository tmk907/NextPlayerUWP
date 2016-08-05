using System;

namespace NextPlayerUWPDataLayer.Model
{
    public class CloudFolder : FolderItem
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public MusicItemTypes Type { get; private set; }

        public CloudFolder()
        {
            folder = "Unknown Folder";
            directory = "";
            songsNumber = 0;
            lastAdded = DateTime.MinValue;
            Id = "";
            Type = MusicItemTypes.unknown;
        }

        public CloudFolder(string folder, string directory, int songsNumber, string id, string parentId, MusicItemTypes type)
        {
            this.folder = folder;
            this.directory = directory;
            this.songsNumber = songsNumber;
            Id = id;
            ParentId = parentId;
            Type = type;
        }

        public override string GetParameter()
        {
            return MusicItemTypes.onedrivefolder + separator + Id;
        }
    }
}
