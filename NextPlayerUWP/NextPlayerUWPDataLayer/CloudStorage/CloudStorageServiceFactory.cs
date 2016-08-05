using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.CloudStorage
{
    public class CloudStorageServiceFactory
    {
        public async Task<List<CloudRootFolder>> GetAllRootFolders()
        {
            List<CloudRootFolder> list = new List<CloudRootFolder>();

            var accounts = CloudAccounts.Instance.GetAllAccounts();
            foreach(var acc in accounts)
            {
                string name = CloudAccounts.GetAccountName(acc.UserName, acc.Type);
                list.Add(new CloudRootFolder(name, acc.UserId,acc.Type));
            }

            return list;
        }

        public ICloudStorageService GetService(CloudStorageType type, string userId)
        {
            ICloudStorageService service = null;
            switch (type)
            {
                //case CloudStorageType.Dropbox:
                //    service = new DropboxStorage.DropboxService(userId);
                //    break;
                //case CloudStorageType.GoogleDrive:
                //    service = new GoogleDrive.GoogleDriveService(userId);
                //    break;
                case CloudStorageType.OneDrive:
                    service = new OneDrive.OneDriveService(userId);
                    break;
                //case CloudStorageType.pCloud:
                //    service = new pCloud.PCloudService(userId);
                //    break;
                default:
                    service = null;
                    break;
            }
            return service;
        }
    }
}
