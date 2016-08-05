//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Drive.v3;
//using Google.Apis.Drive.v3.Data;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;
using NextPlayerUWPDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace NextPlayerUWPDataLayer.CloudStorage.GoogleDrive
{
    public sealed class GoogleDriveService// : ICloudStorageService
    {
        public static event AuthenticationChangeHandler AuthenticationChanged;

        public static void OnAuthenticationChanged(bool isAuthenticated)
        {
            AuthenticationChanged?.Invoke(isAuthenticated);
        }

        private static readonly GoogleDriveService instance = new GoogleDriveService();

        static GoogleDriveService() { }

        public static GoogleDriveService Instance
        {
            get
            {
                return instance;
            }
        }

        private GoogleDriveService()
        {
            System.Diagnostics.Debug.WriteLine("GoogleDriveService()");
            //if (AuthToken != null)
            //{
            //    dropboxClient = new DropboxClient(AuthToken);
            //    OnAuthenticationChanged(true);
            //}
        }

        //private DropboxClient dropboxClient;
        private string ApplicationName = "Next-Player";


        private async Task Authorize()
        {
            //    UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            //        new Uri("ms-appx:///Assets/doc/client_id.json"),
            //        new[] { DriveService.Scope.DriveReadonly },
            //        "user",
            //        CancellationToken.None);


            //    var service = new DriveService(new BaseClientService.Initializer()
            //    {
            //        HttpClientInitializer = credential,
            //        ApplicationName = ApplicationName,
            //    });

            //    FilesResource.ListRequest listRequest = service.Files.List();
            //    listRequest.PageSize = 10;
            //    listRequest.Fields = "nextPageToken, files(id, name)";

            //    // List files.
            //    IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            //    //Console.WriteLine("Files:");
            //    if (files != null && files.Count > 0)
            //    {
            //        foreach (var file in files)
            //        {
            //            //Console.WriteLine("{0} ({1})", file.Name, file.Id);
            //        }
            //    }
            //    else
            //    {
            //        //Console.WriteLine("No files found.");
            //    }
            //    //Console.Read();

        }

        public async Task<bool> Login()
        {
            bool isLoggedIn = false;
            await Authorize();
            return isLoggedIn;
        }

        public async Task Logout()
        {

        }
    }
}
