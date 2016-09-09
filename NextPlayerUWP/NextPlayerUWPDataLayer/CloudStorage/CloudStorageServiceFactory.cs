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
        private List<ICloudStorageService> services = new List<ICloudStorageService>();

        public async Task<List<CloudRootFolder>> GetAllRootFolders()
        {
            List<CloudRootFolder> list = new List<CloudRootFolder>();

            var accounts = CloudAccounts.Instance.GetAllAccounts();
            foreach(var acc in accounts)
            {
                string name = CloudAccounts.GetAccountName(acc.Username, acc.Type);
                list.Add(new CloudRootFolder(name, acc.UserId,acc.Type));
            }

            return list;
        }

        public ICloudStorageService GetService(CloudStorageType type)
        {
            ICloudStorageService service = null;
            switch (type)
            {
                case CloudStorageType.Dropbox:
                    service = new DropboxStorage.DropboxService();
                    break;
                //case CloudStorageType.GoogleDrive:
                //    service = new GoogleDrive.GoogleDriveService(userId);
                //    break;
                case CloudStorageType.OneDrive:
                    service = new OneDrive.OneDriveService();
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

        public ICloudStorageService GetService(CloudStorageType type, string userId)
        {
            ICloudStorageService service = null;
            foreach(var serv in services)
            {
                if (serv.Check(userId, type)) return serv;
            }
            switch (type)
            {
                case CloudStorageType.Dropbox:
                    service = new DropboxStorage.DropboxService(userId);
                    services.Add(service);
                    break;
                //case CloudStorageType.GoogleDrive:
                //    service = new GoogleDrive.GoogleDriveService(userId);
                //    services.Add(service);
                //    break;
                case CloudStorageType.OneDrive:
                    service = new OneDrive.OneDriveService(userId);
                    services.Add(service);
                    break;
                //case CloudStorageType.pCloud:
                //    service = new pCloud.PCloudService(userId);
                //    services.Add(service);
                //    break;
                default:
                    service = null;
                    break;
            }
            return service;
        }
    }
}
