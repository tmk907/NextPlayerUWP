using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TagLib;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Notifications;


namespace NextPlayerUWPDataLayer.Services
{
    public delegate void MediaImportedHandler(string s);

    class ImportedPlaylist
    {
        public string name { get; set; }
        public string path { get; set; }
        public List<string> paths { get; set; }
        public ImportedPlaylist()
        {
            paths = new List<string>();
        }
    }

    public class MediaImport
    {
        public static event MediaImportedHandler MediaImported;

        public static void OnMediaImported(string s)
        {
            MediaImported?.Invoke(s);
        }

        private Dictionary<string, Tuple<int, int>> dbFiles;
        private int songsAdded;
        private IProgress<int> progress;
        private List<string> importedPlaylistPaths;
        private List<ImportedPlaylist> importedPlaylists;

        public static bool IsAudioFile(string type)
        {
            if (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                type == ".wav" || type == ".aac" || type == ".asf" || type == ".flac" ||
                type == ".adt" || type == ".adts" || type == ".amr" || type == ".mp4" ||
                type == ".ogg" || type == ".ape" || type == ".wv" || type == ".opus" ||
                type == ".ac3")
            {
                return true;
            }
            else return false;
        }

        public static bool IsPlaylistFile(string type)
        {
            if (type == ".m3u" || type == ".m3u8" || type == ".wpl" || type == ".pls") return true;
            else return false;
        }

        private async Task AddFilesFromFolder(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            List<SongData> newSongs = new List<SongData>();
            List<int> oldAvailable = new List<int>();
            List<int> toAvailable = new List<int>();

            Tuple<int, int> tuple;//(isAvailable, SongId)
            //.qcp
            //.m4r
            //.3g2
            //.3gp
            //.wm
            //.3gpp
            //.3gp2
            //.mpa
            //.pya
            foreach (var file in files)
            {
                string type = file.FileType.ToLower();
                
                if(type == ".m3u")
                {
                    if (!importedPlaylistPaths.Contains(file.Path))
                    {
                        var ip = await ParseM3UPlaylist(file, folder.Path);
                        importedPlaylists.Add(ip);
                    }
                }
                else if (type == ".m3u8")
                {
                    if (!importedPlaylistPaths.Contains(file.Path))
                    {
                        var ip = await ParseM3UPlaylist(file, folder.Path);
                        importedPlaylists.Add(ip);
                    }
                }
                else if (type == ".wpl")
                {
                    if (!importedPlaylistPaths.Contains(file.Path))
                    {
                        var ip = await ParseWPLPlaylist(file, folder.Path);
                        importedPlaylists.Add(ip);
                    }
                }
                else if (type == ".pls")
                {
                    if (!importedPlaylistPaths.Contains(file.Path))
                    {
                        var ip = await ParsePLSPlaylist(file, folder.Path);
                        importedPlaylists.Add(ip);
                    }
                }
                else if (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                    type == ".wav" || type == ".aac" || type == ".asf" || type == ".flac" ||
                    type == ".adt" || type == ".adts" || type == ".amr" || type == ".mp4" ||
                    type == ".ogg" || type == ".ape" || type == ".wv" || type == ".opus" ||
                    type == ".ac3")
                {
                    if (dbFiles.TryGetValue(file.Path, out tuple))
                    {
                        
                        if (tuple.Item1 == 0) //not available in db
                        {
                            toAvailable.Add(tuple.Item2);
                        }
                        else //available in db
                        {
                            oldAvailable.Add(tuple.Item2);
                        }
                    }
                    else
                    {
                        SongData song = await CreateSongFromFile(file);
                        newSongs.Add(song);
                    }
                }
                else
                {
                    try
                    {
                        string t = file.Path.Substring(file.Path.LastIndexOf('.') + 1);
                    }
                    catch (Exception ex) { }
                }
            }

            await DatabaseManager.Current.UpdateFolderAsync(newSongs, toAvailable, oldAvailable, folder.Path);

            //if (newSongs.Count == 0 && toAvailable.Count == 0 && oldAvailable.Count > 0)
            //{
            //    await DatabaseManager.Current.UpdateFolderAsync(newSongs, toAvailable, oldAvailable, folder.Path);
            //}
            //else
            //{
            //    await DatabaseManager.Current.UpdateFolderAsync(newSongs, toAvailable, oldAvailable, folder.Path);
            //}

            songsAdded += newSongs.Count + toAvailable.Count;
            progress.Report(songsAdded);
            var folders = await folder.GetFoldersAsync();
            foreach (var f in folders)
            {
                await AddFilesFromFolder(f);
            }
        }

        public async Task UpdateDatabase(IProgress<int> p)
        {
            dbFiles = DatabaseManager.Current.GetFilePaths();
            songsAdded = 0;
            progress = p;
            importedPlaylistPaths = await DatabaseManager.Current.GetImportedPlaylistPathsAsync();
            importedPlaylists = new List<ImportedPlaylist>();
            var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.MediaScan, true);
            foreach (var folder in library.Folders)
            {
                await AddFilesFromFolder(folder);
            }

            await DatabaseManager.Current.UpdateTables();

            foreach(var ip in importedPlaylists)
            {
                await SavePlaylist(ip);
            }

            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);
            OnMediaImported("Update");
            SendToast();
        }

        private async Task<SongData> CreateSongFromFile(StorageFile file)
        {
            SongData song = new SongData();
            song.DateAdded = DateTime.Now;
            song.Filename = file.Name;
            song.Path = file.Path;
            song.PlayCount = 0;
            song.LastPlayed = DateTime.MinValue;
            song.IsAvailable = 1;
            song.Tag.Rating = 0;
            song.FileSize = 0;            

            try
            {
                using (Stream fileStream = await file.OpenStreamForReadAsync())
                {
                    try
                    {
                        var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                        try
                        {
                            song.Bitrate = (uint)tagFile.Properties.AudioBitrate;
                            song.Duration = TimeSpan.FromSeconds(Convert.ToInt32(tagFile.Properties.Duration.TotalSeconds));
                        }
                        catch (Exception ex)
                        {
                            song.Duration = TimeSpan.Zero;
                            song.Bitrate = 0;
                        }
                        try
                        {
                            Tag tags;
                            if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v2.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Id3v2);
                                TagLib.Id3v2.PopularimeterFrame pop = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tags, "Windows Media Player 9 Series", false);
                                if (pop != null)
                                {
                                    if (224 <= pop.Rating && pop.Rating <= 255) song.Tag.Rating = 5;
                                    else if (160 <= pop.Rating && pop.Rating <= 223) song.Tag.Rating = 4;
                                    else if (96 <= pop.Rating && pop.Rating <= 159) song.Tag.Rating = 3;
                                    else if (32 <= pop.Rating && pop.Rating <= 95) song.Tag.Rating = 2;
                                    else if (1 <= pop.Rating && pop.Rating <= 31) song.Tag.Rating = 1;
                                }
                            }
                            else if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v1.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Id3v1);
                            }
                            else if (tagFile.TagTypes.ToString().Contains(TagTypes.Apple.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Apple);
                            }
                            else if (tagFile.TagTypes.ToString().Contains(TagTypes.FlacMetadata.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.FlacMetadata);
                            }
                            else if (tagFile.TagTypes.ToString().Contains(TagTypes.Asf.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Asf);
                            }
                            else if (tagFile.TagTypes.ToString().Contains(TagTypes.Ape.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Ape);
                            }
                            else if (tagFile.TagTypes.ToString().Contains(TagTypes.Xiph.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Xiph);
                            }
                            else if (tagFile.TagTypes.ToString().Contains(TagTypes.None.ToString()))
                            {
                                tags = tagFile.GetTag(tagFile.TagTypes);
                            }
                            else
                            {
                                tags = tagFile.GetTag(tagFile.TagTypes);
                            }

                            song.Tag.Album = tags.Album ?? "";
                            song.Tag.AlbumArtist = tags.FirstAlbumArtist ?? "";
                            song.Tag.Artists = tags.JoinedPerformers ?? "";
                            song.Tag.Comment = tags.Comment ?? "";
                            song.Tag.Composers = tags.JoinedComposers ?? "";
                            song.Tag.Conductor = tags.Conductor ?? "";
                            song.Tag.Disc = (int)tags.Disc;
                            song.Tag.DiscCount = (int)tags.DiscCount;
                            song.Tag.FirstArtist = tags.FirstPerformer ?? "";
                            song.Tag.FirstComposer = tags.FirstComposer ?? "";
                            song.Tag.Genres = tags.JoinedGenres ?? "";
                            song.Tag.Lyrics = tags.Lyrics ?? "";
                            song.Tag.Title = tags.Title ?? file.DisplayName;
                            song.Tag.Track = (int)tags.Track;
                            song.Tag.TrackCount = (int)tags.TrackCount;
                            song.Tag.Year = (int)tags.Year;

                            MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                            if (musicProperties.Duration != TimeSpan.Zero)
                            {
                                song.Duration = musicProperties.Duration;
                            }
                            if (song.Tag.Track == 0 && musicProperties.TrackNumber != 0)
                            {
                                song.Tag.Track = (int)musicProperties.TrackNumber;
                            }
                            if (song.Tag.Year == 0 && musicProperties.Year != 0)
                            {
                                song.Tag.Year = (int)musicProperties.Year;
                            }
                            if (song.Tag.Rating == 0 && musicProperties.Rating != 0)
                            {
                                switch (musicProperties.Rating)
                                {
                                    case 99:
                                        song.Tag.Rating = 5;
                                        break;
                                    case 75:
                                        song.Tag.Rating = 4;
                                        break;
                                    case 50:
                                        song.Tag.Rating = 3;
                                        break;
                                    case 25:
                                        song.Tag.Rating = 2;
                                        break;
                                    case 1:
                                        song.Tag.Rating = 1;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (file.FileType == ".wav")
                            {
                                if (musicProperties.Album != "" && musicProperties.Album != song.Tag.Album)
                                {
                                    song.Tag.Album = musicProperties.Album;
                                }
                                if (musicProperties.Title != "" && musicProperties.Title != song.Tag.Title)
                                {
                                    song.Tag.Title = musicProperties.Title;
                                }
                            }
                        }
                        catch (CorruptFileException e)
                        {
                            song.Tag.Album = "";
                            song.Tag.AlbumArtist = "";
                            song.Tag.Artists = "";
                            song.Tag.Comment = "";
                            song.Tag.Composers = "";
                            song.Tag.Conductor = "";
                            song.Tag.Disc = 0;
                            song.Tag.DiscCount = 0;
                            song.Tag.FirstArtist = "";
                            song.Tag.FirstComposer = "";
                            song.Tag.Genres = "";
                            song.Tag.Lyrics = "";
                            song.Tag.Title = file.DisplayName;
                            song.Tag.Track = 0;
                            song.Tag.TrackCount = 0;
                            song.Tag.Year = 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        song.Tag.Album = "";
                        song.Tag.AlbumArtist = "";
                        song.Tag.Artists = "";
                        song.Tag.Comment = "";
                        song.Tag.Composers = "";
                        song.Tag.Conductor = "";
                        song.Tag.Disc = 0;
                        song.Tag.DiscCount = 0;
                        song.Tag.FirstArtist = "";
                        song.Tag.FirstComposer = "";
                        song.Tag.Genres = "";
                        song.Tag.Lyrics = "";
                        song.Tag.Title = file.DisplayName;
                        song.Tag.Track = 0;
                        song.Tag.TrackCount = 0;
                        song.Tag.Year = 0;
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Logger.Save("CreateSongFromFile FileNotFound" + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
                song.FileSize = 0;
                song.IsAvailable = 0;

                song.Duration = TimeSpan.Zero;
                song.Bitrate = 0;
                song.Tag.Album = "";
                song.Tag.AlbumArtist = "";
                song.Tag.Artists = "";
                song.Tag.Comment = "";
                song.Tag.Composers = "";
                song.Tag.Conductor = "";
                song.Tag.Disc = 0;
                song.Tag.DiscCount = 0;
                song.Tag.FirstArtist = "";
                song.Tag.FirstComposer = "";
                song.Tag.Genres = "";
                song.Tag.Lyrics = "";
                song.Tag.Title = file.DisplayName;
                song.Tag.Track = 0;
                song.Tag.TrackCount = 0;
                song.Tag.Year = 0;

                return song;
            }
            catch (Exception ex)
            {
                Logger.Save("CreateSongFromFile" + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
                song.FileSize = 0;
            }
            return song;
        }

        public async Task<SongItem> OpenSingleFileAsync(StorageFile file)
        {
            string type = file.FileType.ToLower();
            SongItem song = new SongItem();
            if (IsAudioFile(type))
            {
                song = await DatabaseManager.Current.GetSongItemIfExistAsync(file.Path);
                if (song == null)
                {
                    var songData = await CreateSongFromFile(file);
                    songData.IsAvailable = 0;
                    int id = await DatabaseManager.Current.InsertSongAsync(songData);
                    song = await DatabaseManager.Current.GetSongItemAsync(id);
                    string token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    
                    await FutureAccessHelper.SaveToken(file.Path, token);
                }
                song.SourceType = Enums.MusicSource.LocalNotLibrary;
            }
            return song;
        }

        public async Task<IEnumerable<SongItem>> OpenPlaylistFileAsync(StorageFile file)
        {
            List<SongItem> songs = new List<SongItem>();
            string type = file.FileType.ToLower();

            if (IsPlaylistFile(type))
            {
                string folderPath = Path.GetDirectoryName(file.Path);
                var f = await file.GetParentAsync();
                ImportedPlaylist playlist = new ImportedPlaylist();
                if (file.Path != folderPath)
                {

                }
                if (type == ".m3u")
                {
                    playlist = await ParseM3UPlaylist(file, folderPath);
                }
                else if (type == ".m3u8")
                {
                    playlist = await ParseM3UPlaylist(file, folderPath);
                }
                else if (type == ".wpl")
                {
                    playlist = await ParseWPLPlaylist(file, folderPath);
                }
                else if (type == ".pls")
                {
                    playlist = await ParsePLSPlaylist(file, folderPath);
                }
                if (playlist.paths.Count > 0)
                {
                    songs = await DatabaseManager.Current.GetSongItemsForPlaylistAsync(playlist.paths);
                }
            }
            return songs;
        }

        private void SendToast()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(loader.GetString("LibraryUpdated")));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(loader.GetString("NewSongs")+" "+ songsAdded));
            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddMinutes(2);
            try
            {
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {

            }
        }

        public static async Task UpdateCacheTables()
        {
            await DatabaseManager.Current.UpdateTables();

        }

        public static async Task UpdateRating(int songId, int rating)
        {

        }
        public static async Task UpdateLyrics(int songId, string lyrics)
        {

        }
        public static async Task UpdateFileTags(SongData songData)
        {

        }

        private async Task<ImportedPlaylist> ParseM3UPlaylist(StorageFile file, string folderPath)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.name = file.DisplayName;
            iplaylist.path = file.Path;
            //string folderPath = Path.GetDirectoryName(file.Path);
            using (var stream = await file.OpenStreamForReadAsync())
            {
                StreamReader streamReader = new StreamReader(stream);
                bool isExtended = false;
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line.StartsWith("#"))
                    {
                        if (line.StartsWith("#EXTM3U"))
                        {
                            isExtended = true;
                        }
                        else if (line.StartsWith("#EXTINF"))
                        {

                        }
                    }
                    else if (line.StartsWith("http"))
                    {

                    }
                    else
                    {
                        iplaylist.paths.Add(CreateFullFilePath(line, folderPath));
                    }
                }
            }
            return iplaylist;
        }

        private async Task<ImportedPlaylist> ParseWPLPlaylist(StorageFile file, string folderPath)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.name = file.DisplayName;
            iplaylist.path = file.Path;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                try
                {
                    var doc = XDocument.Load(stream).Descendants("body").Elements("seq").Elements("media");
                    foreach (var media in doc)
                    {
                        var src = media.Attribute("src").Value;
                        iplaylist.paths.Add(CreateFullFilePath(src, folderPath));
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return iplaylist;
        }

        private async Task<ImportedPlaylist> ParsePLSPlaylist(StorageFile file, string folderPath)
        {
            ImportedPlaylist iplaylist = new ImportedPlaylist();
            iplaylist.name = file.DisplayName;
            iplaylist.path = file.Path;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                StreamReader streamReader = new StreamReader(stream);
                if (!streamReader.EndOfStream)
                {
                    string header = streamReader.ReadLine();
                    if (header != "[playlist]")
                    {
                        return iplaylist;
                    }
                }
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line.StartsWith("File"))
                    {
                        string path = line.Substring(line.IndexOf('=') + 1);
                        if (path.StartsWith("http"))
                        {

                        }
                        else
                        {
                            iplaylist.paths.Add(CreateFullFilePath(path, folderPath));
                        }
                    }
                    
                }
            }
            return iplaylist;
        }

        private async Task SavePlaylist(ImportedPlaylist iplaylist)
        {
            if (iplaylist.paths.Count > 0)
            {
                string folderPath = Path.GetDirectoryName(iplaylist.path);
                var songs = await DatabaseManager.Current.GetSongItemsForPlaylistAsync(iplaylist.paths);
                //var res = songs.Where(s => iplaylist.paths.Contains(s.Path));
                if (songs.Count() == 0)
                {
                    DatabaseManager.Current.InsertImportedPlaylist(iplaylist.name, iplaylist.path, -1);
                }
                else
                {
                    int plainId = DatabaseManager.Current.InsertPlainPlaylist(iplaylist.name);
                    int id = DatabaseManager.Current.InsertImportedPlaylist(iplaylist.name, iplaylist.path, plainId);
                    int place = 0;
                    foreach(var r in songs)
                    {
                        await DatabaseManager.Current.InsertPlainPlaylistEntryAsync(plainId, r.SongId, place);
                        place++;
                    }
                }
                
            }
            else
            {
                DatabaseManager.Current.InsertImportedPlaylist(iplaylist.name, iplaylist.path, -1);
            }
        }

        private static string CreateFullFilePath(string filePath, string folderPath)
        {
            string fullpath = "";
            if (filePath.StartsWith(@"\"))
            {
                fullpath = folderPath + filePath;
            }
            else
            {
                bool isRooted = false;
                try
                {
                    isRooted = Path.IsPathRooted(filePath);
                }
                catch (Exception) { }
                if (isRooted)
                {
                    fullpath = filePath;
                }
                else if (filePath.StartsWith(@"..\"))
                {
                    try
                    {
                        fullpath = Path.GetFullPath(folderPath + @"\" + filePath);
                    }
                    catch (Exception) { }
                }
                else
                {
                    fullpath = folderPath + @"\" + filePath;
                }
            }
            return fullpath;
        }
    }
}
