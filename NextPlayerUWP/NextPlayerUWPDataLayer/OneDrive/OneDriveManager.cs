using Microsoft.OneDrive.Sdk;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.OneDrive
{
    public sealed class OneDriveManager
    {
        private static readonly OneDriveManager instance = new OneDriveManager();

        static OneDriveManager() { }

        public static OneDriveManager Instance
        {
            get
            {
                return instance;
            }
        }

        private OneDriveManager()
        {
            
        }

        private readonly string[] scopes = new string[] { "onedrive.readonly", "wl.signin" };
        public IOneDriveClient oneDriveClient { get; set; }

        public bool IsAuthenticated
        {
            get { return (oneDriveClient != null && oneDriveClient.IsAuthenticated); }
        }

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
            //string id = await GetMusicFolderId();
            //if (String.IsNullOrEmpty(id)) return;
            //await GetFolderContent(id);
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

        public async Task<string> GetMusicFolderId()
        {
            var rootChildrens = await oneDriveClient.Drive.Root.Children.Request().GetAsync();
            return rootChildrens.FirstOrDefault(i => i.SpecialFolder.Name.Equals("music"))?.Id;
        }

        public async Task<SongItem> GetSongTest()
        {
            var id = await GetMusicFolderId();
            var children = await oneDriveClient.Drive.Items[id].Children.Request().GetAsync();
            var song = children.FirstOrDefault(s => s.Name.Contains("G.R"));// != null);
            var si = new SongItem();
            si.Path = song.AdditionalData["@content.downloadUrl"].ToString();
            si.Title = song.Name;
            si.Album = song?.Audio?.Album ?? "test";
            si.Artist = song?.Audio?.Artist ?? "test";
            si.CoverPath = song?.Thumbnails?.FirstOrDefault()?.Medium?.Url ?? si.CoverPath;
            si.IsAlbumArtSet = true;
            si.SourceType = Enums.MusicSource.OneDrive;
            return si;
        }

        private Dictionary<string, IChildrenCollectionPage> cache = new Dictionary<string, IChildrenCollectionPage>();

        public async Task<List<SongItem>> GetSongItemsFromItem(string id)
        {
            List<SongItem> songs = new List<SongItem>();
            IChildrenCollectionPage children;
            if (cache.ContainsKey(id))
            {
                children = cache[id];
            }
            else
            {
                children = await oneDriveClient.Drive.Items[id].Children.Request().GetAsync();
                cache.Add(id, children);
            }
            foreach (var item in children)
            {
                if (item.Audio != null)
                {
                    songs.Add(OneDriveItemToSongItem(item));
                }
            }
            return songs;
        }

        public async Task<List<OneDriveFolder>> GetFoldersFromItem(string id)
        {
            List<OneDriveFolder> folders = new List<OneDriveFolder>();
            IChildrenCollectionPage children;
            if (cache.ContainsKey(id))
            {
                children = cache[id];
            }
            else
            {
                children = await oneDriveClient.Drive.Items[id].Children.Request().GetAsync();
                cache.Add(id, children);
            }
            foreach (var item in children)
            {
                if (item.Folder != null)
                {
                    folders.Add(new OneDriveFolder(item.Name, "", item.Folder.ChildCount ?? 0, item.Id));
                }
            }
            return folders;
        }

        private static SongItem OneDriveItemToSongItem(Item item)
        {
            SongItem song = new SongItem();
            song.Album = item?.Audio.Album ?? "";
            song.AlbumArtist = item?.Audio.AlbumArtist ?? "";
            song.Artist = item?.Audio.Artist ?? "";
            song.Composer = item?.Audio.Composers ?? "";
            //song.CoverPath = item?.Audio. ?? "";
            //song.DateAdded = item.CreatedDateTime.Value.DateTime;
            song.Disc = item?.Audio.Disc ?? 0;
            song.Duration = (item?.Audio.Duration != null) ? TimeSpan.FromMilliseconds((double)item.Audio.Duration) : TimeSpan.Zero;
            song.Genres = item?.Audio.Genre ?? "";
            song.Path = item.AdditionalData["@content.downloadUrl"].ToString();
            song.SourceType = Enums.MusicSource.OneDrive;
            song.Title = String.IsNullOrEmpty(item?.Audio.Title) ? item.Name : item?.Audio.Title;
            song.TrackNumber = item?.Audio.Track ?? 0;
            song.Year = item?.Audio.Year ?? 0;
            song.GenerateID();
            return song;
        }

    }
}
