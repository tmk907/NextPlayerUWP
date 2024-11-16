using NextPlayerUWPDataLayer.CloudStorage;
using System;

namespace NextPlayerUWPDataLayer.Model
{
    public class CloudFolder : CloudRootFolder
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public MusicItemTypes MusicType { get; private set; }

        public CloudFolder()
        {
            folder = "Unknown Folder";
            directory = "";
            songsNumber = 0;
            lastAdded = DateTime.MinValue;

            UserId = "";
            CloudType = CloudStorageType.Unknown;

            Id = "";
            ParentId = "";
            MusicType = MusicItemTypes.unknown;
        }

        public CloudFolder(string folder, string directory, int songsNumber, string id, string parentId, CloudStorageType type, string userId)
        {
            this.folder = folder;
            this.directory = directory;
            this.songsNumber = songsNumber;
            lastAdded = DateTime.Now;

            UserId = userId;
            CloudType = type;

            Id = id;
            ParentId = parentId;
            switch (type)
            {
                case CloudStorageType.Dropbox:
                    MusicType = MusicItemTypes.dropboxfolder;
                    break;
                case CloudStorageType.GoogleDrive:
                    MusicType = MusicItemTypes.googledrivefolder;
                    break;
                case CloudStorageType.OneDrive:
                    MusicType = MusicItemTypes.onedrivefolder;
                    break;
                case CloudStorageType.pCloud:
                    MusicType = MusicItemTypes.pcloudfolder;
                    break;
                default:
                    MusicType = MusicItemTypes.unknown;
                    break;
            }
        }

        public override string GetParameter()
        {
            return MusicType + separator + UserId + separator + Id;
        }
    }
}