using NextPlayerUWPDataLayer.CloudStorage;
using System;

namespace NextPlayerUWPDataLayer.Model
{
    public class CloudRootFolder : FolderItem
    {
        public string UserId { get; set; }
        public CloudStorageType CloudType { get; set; }

        public CloudRootFolder()
        {
            folder = "";
            UserId = "";
            CloudType = CloudStorageType.Unknown;
        }

        public CloudRootFolder(string name, string userId, CloudStorageType type)
        {
            folder = name;
            UserId = userId;
            CloudType = type;
        }

        public static string ToParameter(string userId, CloudStorageType type)
        {
            return "root" + "%%%" + ((int)type).ToString() + "%%%" + userId;
        }

        public static CloudStorageType ParameterToType(string param)
        {
            var s = param.Split(new string[] { "%%%" }, StringSplitOptions.RemoveEmptyEntries);
            return (CloudStorageType)Int32.Parse(s[1]);
        }

        public static string ParameterToUserId(string param)
        {
            var s = param.Split(new string[] { "%%%" }, StringSplitOptions.RemoveEmptyEntries);
            return s[2];
        }

        public static bool IsCloudRootFolderParameter(string param)
        {
            return param.StartsWith("root");
        }
    }
}
