using Microsoft.OneDrive.Sdk.Authentication;
using System;
using Windows.Security.Credentials;

namespace NextPlayerUWPDataLayer.CloudStorage.OneDrive
{
    public class OneDriveCredentialVault : ICredentialVault
    {
        private string VaultResourceName = "";
        private string UserId { get; set; }

        public OneDriveCredentialVault(string vaultName, string userId)
        {
            if (string.IsNullOrEmpty(vaultName))
            {
                throw new ArgumentException("You must provide a vault name");
            }
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("You must provide a userId");
            }

            this.VaultResourceName = vaultName;
            this.UserId = userId;
        }

        public void AddCredentialCacheToVault(CredentialCache credentialCache)
        {
            this.DeleteStoredCredentialCache();

            var vault = new PasswordVault();
            var cred = new PasswordCredential(
                this.VaultResourceName,
                this.UserId,
                Convert.ToBase64String(credentialCache.GetCacheBlob()));
            vault.Add(cred);
        }

        public bool RetrieveCredentialCache(CredentialCache credentialCache)
        {
            var creds = this.RetrieveCredentialFromVault();

            if (creds != null)
            {
                credentialCache.InitializeCacheFromBlob(Convert.FromBase64String(creds.Password));
                return true;
            }

            return false;
        }

        public bool DeleteStoredCredentialCache()
        {
            var creds = this.RetrieveCredentialFromVault();

            if (creds != null)
            {
                var vault = new PasswordVault();
                vault.Remove(creds);
                return true;
            }

            return false;
        }

        private PasswordCredential RetrieveCredentialFromVault()
        {
            var vault = new PasswordVault();
            PasswordCredential creds = null;

            try
            {
                creds = vault.Retrieve(this.VaultResourceName, this.UserId);
            }
            catch (Exception)
            {
                // This happens when the vault is empty. Swallow.
            }

            return creds;
        }
    }
}
