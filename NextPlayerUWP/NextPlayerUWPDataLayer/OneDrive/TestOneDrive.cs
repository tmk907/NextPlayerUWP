using Microsoft.OneDrive.Sdk;
using NextPlayerUWPDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.OneDrive
{
    public class TestOneDrive
    {
        private readonly string[] scopes = new string[] { "onedrive.readonly", "wl.signin" };
        public IOneDriveClient oneDriveClient { get; set; }

        public async Task Test()
        {
            //var oneDriveClient = await OneDriveClientExtensions.GetAuthenticatedUniversalClient(scopes);
            //await oneDriveClient.AuthenticateAsync();

            if (oneDriveClient != null)
            {
                await oneDriveClient.SignOutAsync();

                var client = oneDriveClient as OneDriveClient;
                if (client != null)
                {
                    client.Dispose();
                }

                oneDriveClient = null;
            }

            await InitializeClient(ClientType.Consumer);
            string id = await GetMusicFolderId();
            if (String.IsNullOrEmpty(id)) return;
            await GetFolderContent(id);
            //IEnumerable<Item> items = item == null ? new List<Item>() : item.Children.CurrentPage.Where(child => child.Folder != null);

            var root = await oneDriveClient.Drive.Root.Request().GetAsync();
            
            //var items = await oneDriveClient.Drive.Items.Request().GetAsync();

            //await oneDriveClient.SignOutAsync();
        }

        private async Task InitializeClient(ClientType clientType)
        {
            if (oneDriveClient == null)
            {
                OneDriveClient client = null;

                try
                {

                    //var client1 = OneDriveClient.GetMicrosoftAccountClient(AppConstants.OneDriveAppId, "", scopes, serviceInfoProvider: new ServiceInfoProvider { UserSignInName });

                    client = OneDriveClientExtensions.GetUniversalClient(scopes) as OneDriveClient;
                    
                    var x = await client.AuthenticateAsync();
                    
                    

                    oneDriveClient = client;

                    

                    //NavigationStack.Add(new ItemModel(new Item()));
                    //Frame.Navigate(typeof(MainPage), e);
                }
                catch (OneDriveException exception)
                {
                    // Swallow the auth exception but write message for debugging.
                    Debug.WriteLine(exception.Error.Message);

                    if (client != null)
                    {
                        client.Dispose();
                    }
                }
            }
            else
            {
                //Frame.Navigate(typeof(MainPage), e);
            }
        }

        private async Task<string> GetMusicFolderId()
        {
            var rootChildrens = await oneDriveClient.Drive.Root.Children.Request().GetAsync();
            return rootChildrens.FirstOrDefault(i => i.SpecialFolder.Name.Equals("music"))?.Id;
        }

        private async Task GetFolderContent(string id)
        {
            var children = await oneDriveClient.Drive.Items[id].Children.Request().GetAsync();
            foreach(var c in children)
            {
               
            }
        }

    }
}
