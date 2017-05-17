using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerUWP.Common
{
    public class FoldersVMHelper
    {
        public FoldersVMHelper()
        {
            MediaImport.MediaImported += MediaImport_MediaImported;
        }

        public class TreeNode
        {
            public string Path { get; set; } = "";
        }

        public class TreeFolderNode : TreeNode
        {
            //public List<TreeFileNode> Files { get; set; } = new List<TreeFileNode>();
            public ObservableCollection<SongItem> Songs { get; set; } = new ObservableCollection<SongItem>();
            public Dictionary<string, TreeFolderNode> Folders { get; set; } = new Dictionary<string, TreeFolderNode>();
            public string FolderName { get; set; } = "";
            public DateTime LastAdded { get; set; } = DateTime.MinValue;
            public int SongsCount { get; set; } = 0;
        }

        //public class TreeFileNode : TreeNode
        //{
        //    public string FileName { get; set; } = "";
        //}

        private TreeFolderNode root;
        private List<string> rootPaths;

        public async Task Initialize()
        {
            if (root == null)
            {
                root = new TreeFolderNode();
                rootPaths = new List<string>();
                bool subfolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders);
                var allFolders = await DatabaseManager.Current.GetFolderItemsAsync();

                if (allFolders.Count > 0)
                {
                    string rootPath = "!";
                    var c = root;
                    foreach (var f1 in allFolders.OrderBy(f => f.Directory))
                    {
                        if (!f1.Directory.StartsWith(rootPath))
                        {
                            //if (subfolders) f1.SongsNumber = folders.Where(f => f.Directory.StartsWith(f1.Directory)).Sum(g => g.SongsNumber);
                            rootPath = f1.Directory;
                            rootPaths.Add(rootPath);
                            root.Folders.Add(f1.Directory, new TreeFolderNode()
                            {
                                FolderName = f1.Folder,
                                Path = f1.Directory,
                                LastAdded = f1.LastAdded,
                                SongsCount = f1.SongsNumber
                            });
                            c = root.Folders[f1.Directory];
                        }
                        else
                        {
                            var subf = f1.Directory.Remove(0, rootPath.Length + 1).Split(new char[] { '\\' });
                            string path = rootPath;
                            var temp = c;
                            for (int i = 0; i < subf.Length; i++)
                            {
                                path += "\\" + subf[i];
                                if (temp.Folders.ContainsKey(subf[i]))
                                {
                                    temp = temp.Folders[subf[i]];
                                }
                                else
                                {
                                    var folderNode = new TreeFolderNode()
                                    {
                                        FolderName = subf[i],
                                        Path = path,
                                        LastAdded = f1.LastAdded,
                                        SongsCount = f1.SongsNumber
                                    };
                                    temp.Folders.Add(subf[i], folderNode);
                                    temp = folderNode;
                                }
                            }
                        }
                    }
                }

                var aa = GetRootFolders();
                var bb = GetSubFolders(@"D:\Muzyka\Andrea Bocelli");
                var cc = GetSubFolders(@"D:\Muzyka\Filmowa\Z Gier");
                var dd = GetSubFolders(@"C:\Users\tomek\Music");
            }
        }

        public ObservableCollection<FolderItem> GetRootFolders()
        {
            ObservableCollection<FolderItem> folders = new ObservableCollection<FolderItem>();
            foreach(var folder in root.Folders)
            {
                folders.Add(ToFolderItem(folder.Value));
            }
            return folders;
        }

        public ObservableCollection<FolderItem> GetSubFolders(string folderDirectory)
        {
            ObservableCollection<FolderItem> folders = new ObservableCollection<FolderItem>();
            var node = FindFolderNode(folderDirectory);
            foreach (var folder in node.Folders)
            {
                folders.Add(ToFolderItem(folder.Value));
            }
            return folders;
        }

        public async Task<ObservableCollection<SongItem>> GetSongsFromFolder(string folderDirectory)
        {
            var node = FindFolderNode(folderDirectory);

            if (node.Songs.Count == 0)
            {
                node.Songs = await DatabaseManager.Current.GetSongItemsFromFolderAsync(folderDirectory);
            }
            return node.Songs;
        }

        public void ClearCachedSongs()
        {
            foreach(var folder in root.Folders)
            {
                ClearCachedSongs(folder.Value);
            }
        }

        private void ClearCachedSongs(TreeFolderNode node)
        {
            node.Songs = new ObservableCollection<SongItem>();
            foreach(var folder in node.Folders)
            {
                ClearCachedSongs(folder.Value);
            }
        }

        private TreeFolderNode FindFolderNode(string folderDirectory)
        {
            var rootPath = GetRootPath(folderDirectory);
            var node = root.Folders[rootPath];
            var array = folderDirectory.Remove(0, rootPath.Length).Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var foldername in array)
            {
                node = node.Folders[foldername];
            }
            return node;
        }

        private string GetRootPath(string directory)
        {
            foreach(var rootPath in rootPaths)
            {
                if (directory.StartsWith(rootPath)) return rootPath;
            }
            return "";
        }

        private FolderItem ToFolderItem(TreeFolderNode node)
        {
            return new FolderItem(node.FolderName, node.Path)
            {
                LastAdded = node.LastAdded,
                SongsNumber = node.SongsCount
            };
        }

        private int CountSubfolders(TreeFolderNode node)
        {
            if (node.Folders.Count == 0)
            {
                return 1;
            }
            else
            {
                int sum = 1;
                foreach (var folder in node.Folders)
                {
                    int count = CountSubfolders(folder.Value);
                    System.Diagnostics.Debug.WriteLine("Suma {0} {1}", count, folder.Value.Path);
                    sum += count;
                }
                return sum;
            }
        }

        private async void MediaImport_MediaImported(string s)//TODO
        {
            //Template10.Common.IDispatcherWrapper d = Dispatcher;
            //if (d == null)
            //{
            //    d = Template10.Common.WindowWrapper.Current().Dispatcher;
            //}
            //if (d == null)
            //{
            //    NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("FoldersViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
            //    TelemetryAdapter.TrackEvent("Dispatcher null");
            //    return;
            //}
            //await d.DispatchAsync(() => ReloadData());
        }
    }
}
