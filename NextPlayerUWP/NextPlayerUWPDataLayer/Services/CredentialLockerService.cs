using System;
using System.Collections.Generic;
using Windows.Security.Credentials;

namespace NextPlayerUWPDataLayer.Services
{
    public class CredentialLockerService
    {
        private const string DefaultResourceName = "Next-Player_CredentialLockerService";
        private string VaultResourceName = "";
        public const string LastFmVault = "Next-Player_LastFm";
        public const string DropboxVault = "Next-Player_Dropbox";
        public const string PCloudVault = "Next-Player_pCloud";

        public CredentialLockerService()
        {
            VaultResourceName = DefaultResourceName;
        }

        public CredentialLockerService(string vaultResourceName)
        {
            VaultResourceName = vaultResourceName;
        }

        public void AddCredentials(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(password)) return;
            DeleteCredentials(userName);
            var vault = new PasswordVault();
            var cred = new PasswordCredential(VaultResourceName, userName, password);
            vault.Add(cred);
        }

        public string GetPassword(string userName)
        {
            var vault = new PasswordVault();
            try
            {
                var cred = vault.Retrieve(VaultResourceName, userName);
                return cred.Password;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public void DeleteCredentials(string userName)
        {
            var vault = new PasswordVault();
            try
            {
                var cred = vault.Retrieve(VaultResourceName, userName);
                vault.Remove(cred);
            }
            catch (Exception)
            {
            }
        }

        public void DeleteAllCredentials()
        {
            var vault = new PasswordVault();
            var creds = vault.FindAllByResource(VaultResourceName);
            foreach(var cred in creds)
            {
                vault.Remove(cred);
            }
        }

        public List<string> GetUserNames()
        {
            var vault = new PasswordVault();
            List<string> list = new List<string>(); 
            var creds = vault.FindAllByResource(VaultResourceName);
            foreach(var cred in creds)
            {
                list.Add(cred.UserName);
            }
            return list;
        }

        public string GetFirstUserName()
        {
            var vault = new PasswordVault();
            try
            {
                var creds = vault.FindAllByResource(VaultResourceName);
                if (creds.Count > 0)
                {
                    return creds[0].UserName;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
