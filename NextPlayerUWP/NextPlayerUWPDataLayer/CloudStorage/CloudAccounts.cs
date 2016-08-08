using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.CloudStorage
{
    public sealed class CloudAccounts
    {
        private static readonly CloudAccounts instance = new CloudAccounts();

        static CloudAccounts() { }

        public static CloudAccounts Instance
        {
            get
            {
                return instance;
            }
        }

        private CloudAccounts()
        {
            accounts = DatabaseManager.Current.GetAllCloudAccounts();
        }

        private List<CloudAccount> accounts;

        public async Task<CloudAccount> AddAccount(string userId, CloudStorageType type, string username)
        {
            int id = await DatabaseManager.Current.AddCloudAccountAsync(userId, type, username);
            CloudAccount ca = new CloudAccount(id, userId, type, username);
            accounts.Add(ca);
            return ca;
        }

        public async Task DeleteAccount(CloudAccount account)
        {
            if (account == null) return;
            await DatabaseManager.Current.DeleteCloudAccountAsync(account);
            accounts.Remove(account);
        }

        public async Task DeleteAccount(string userId, CloudStorageType type)
        {
            var account = accounts.FirstOrDefault(a => a.UserId.Equals(userId) && a.Type.Equals(type));
            await DeleteAccount(account);
        }

        public List<CloudAccount> GetAllAccounts()
        {
            List<CloudAccount> list = new List<CloudAccount>();
            foreach(var acc in accounts)
            {
                list.Add(acc);
            }
            return list;
        }

        public CloudAccount GetAccount(string userId)
        {
            return accounts.FirstOrDefault(a => a.UserId.Equals(userId));
        }

        public List<CloudAccount> GetAccountsByType(CloudStorageType type)
        {
            List<CloudAccount> list = new List<CloudAccount>();
            foreach (var acc in accounts.Where(a=>a.Type.Equals(type)))
            {
                list.Add(acc);
            }
            return list;
        }

        public bool Exists(string userId, CloudStorageType type)
        {
            return accounts.Exists(a => a.UserId.Equals(userId) && a.Type.Equals(type));
        }

        public static string GetAccountName(string userName, CloudStorageType type)
        {
            string name = "";
            switch (type)
            {
                case CloudStorageType.Dropbox:
                    name = "Dropbox";
                    break;
                case CloudStorageType.GoogleDrive:
                    name = "Google Drive";
                    break;
                case CloudStorageType.OneDrive:
                    name = "OneDrive";
                    break;
                case CloudStorageType.pCloud:
                    name = "pCloud";
                    break;
                default:
                    name = "Unknown";
                    break;
            }
            name += " " + userName;
            return name;
        }
    }
}
