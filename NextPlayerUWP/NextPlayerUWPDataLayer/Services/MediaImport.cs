using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TagLib;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Notifications;


namespace NextPlayerUWPDataLayer.Services
{
    public delegate void MediaImportedHandler(string s);

    public class MediaImport
    {
        public static event MediaImportedHandler MediaImported;

        private class ImportedPlaylistComparer : IEqualityComparer<ImportedPlaylist>
        {
            public bool Equals(ImportedPlaylist x, ImportedPlaylist y)
            {
                return (x.Path == y.Path);
            }

            public int GetHashCode(ImportedPlaylist obj)
            {
                return obj.Path.GetHashCode();
            }
        }

        public static void OnMediaImported(string s)
        {
            MediaImported?.Invoke(s);
        }

        public MediaImport(FileFormatsHelper fileFormatsHelper)
        {
            this.fileFormatsHelper = fileFormatsHelper;
        }

        private FileFormatsHelper fileFormatsHelper;
        string currentScanningFolderPath;
        private int songsAdded = 0;
        private int modifiedSongs;
        private int playlistsAdded = 0;
        private long updateDurationSeconds = 0;
        private IProgress<string> progress;
        private List<ImportedPlaylist> oldImportedPlaylists;
        private List<ImportedPlaylist> newImportedPlaylists;
        private List<ImportedPlaylist> updatedPlaylists;
        private List<ImportedPlaylist> withoutChangePlaylists;
        private List<string> libraryDirectories;
        private List<SdCardFolder> sdCardFoldersToScan;
        private QueryOptions queryOptions;
        private List<StorageFile> playlistFiles;

        private async Task UpdateSongMusicPropertiesAsync(Tables.SongsTable songTable, BasicProperties prop, StorageFile file)
        {
            songTable.FileSize = (long)prop.Size;
            songTable.DateModified = prop.DateModified.UtcDateTime;
            try
            {
                using (Stream fileStream = await file.OpenStreamForReadAsync())
                {
                    try
                    {
                        var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                        try
                        {
                            Tag tags;
                            if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v2.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Id3v2);
                                if (songTable.Rating == 0)
                                {
                                    TagLib.Id3v2.PopularimeterFrame pop = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tags, "Windows Media Player 9 Series", false);
                                    if (pop != null)
                                    {
                                        if (224 <= pop.Rating && pop.Rating <= 255) songTable.Rating = 5;
                                        else if (160 <= pop.Rating && pop.Rating <= 223) songTable.Rating = 4;
                                        else if (96 <= pop.Rating && pop.Rating <= 159) songTable.Rating = 3;
                                        else if (32 <= pop.Rating && pop.Rating <= 95) songTable.Rating = 2;
                                        else if (1 <= pop.Rating && pop.Rating <= 31) songTable.Rating = 1;
                                    }
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

                            if (!String.IsNullOrWhiteSpace(tags.Album)) songTable.Album = tags.Album;
                            if (!String.IsNullOrWhiteSpace(tags.FirstAlbumArtist)) songTable.AlbumArtist = tags.FirstAlbumArtist;
                            if (!String.IsNullOrWhiteSpace(tags.JoinedPerformers)) songTable.Artists = tags.JoinedPerformers;
                            if (!String.IsNullOrWhiteSpace(tags.Comment)) songTable.Comment = tags.Comment;
                            if (!String.IsNullOrWhiteSpace(tags.JoinedComposers)) songTable.Composers = tags.JoinedComposers;
                            if (!String.IsNullOrWhiteSpace(tags.Conductor)) songTable.Conductor = tags.Conductor;
                            songTable.Disc = (tags.Disc != 0) ? (int)tags.Disc : songTable.Disc;
                            songTable.DiscCount = (tags.DiscCount != 0) ? (int)tags.DiscCount : songTable.DiscCount;
                            if (!String.IsNullOrWhiteSpace(tags.FirstPerformer)) songTable.FirstArtist = tags.FirstPerformer;
                            if (!String.IsNullOrWhiteSpace(tags.FirstComposer)) songTable.FirstComposer = tags.FirstComposer;
                            if (!String.IsNullOrWhiteSpace(tags.JoinedGenres)) songTable.Genres = tags.JoinedGenres;
                            if (!String.IsNullOrWhiteSpace(tags.Lyrics)) songTable.Lyrics = tags.Lyrics;
                            if (!String.IsNullOrWhiteSpace(tags.Title)) songTable.Title = tags.Title;
                            songTable.Track = (tags.Track != 0) ? (int)tags.Track : songTable.Track;
                            songTable.TrackCount = (tags.TrackCount != 0) ? (int)tags.TrackCount : songTable.TrackCount;
                            songTable.Year = (tags.Year != 0) ? (int)tags.Year : songTable.Year;

                            MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                            if (songTable.Track == 0 && musicProperties.TrackNumber != 0)
                            {
                                songTable.Track = (int)musicProperties.TrackNumber;
                            }
                            if (songTable.Year == 0 && musicProperties.Year != 0)
                            {
                                songTable.Year = (int)musicProperties.Year;
                            }
                            if (songTable.Rating == 0 && musicProperties.Rating != 0)
                            {
                                switch (musicProperties.Rating)
                                {
                                    case 99:
                                        songTable.Rating = 5;
                                        break;
                                    case 75:
                                        songTable.Rating = 4;
                                        break;
                                    case 50:
                                        songTable.Rating = 3;
                                        break;
                                    case 25:
                                        songTable.Rating = 2;
                                        break;
                                    case 1:
                                        songTable.Rating = 1;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (file.FileType == ".wav")
                            {
                                if (musicProperties.Album != "" && musicProperties.Album != songTable.Album)
                                {
                                    songTable.Album = musicProperties.Album;
                                }
                                if (musicProperties.Title != "" && musicProperties.Title != songTable.Title)
                                {
                                    songTable.Title = musicProperties.Title;
                                }
                            }
                        }
                        catch (CorruptFileException e)
                        {
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Logger2.Current.WriteMessage("CreateSongFromFile FileNotFound" + Environment.NewLine + ex.Message, Logger2.Level.Information);
            }
            catch (Exception ex)
            {
                Logger2.Current.WriteMessage("CreateSongFromFile" + Environment.NewLine + ex.Message, Logger2.Level.Information);
            }
        }

        private async Task AddFilesFromFolderAsync(StorageFolder folder, bool includeSubFolders = true)
        {
            progress.Report(folder.Path + "|" + songsAdded);
            var query = folder.CreateFileQueryWithOptions(queryOptions);
            var files = await query.GetFilesAsync();

            List<SongData> newSongs = new List<SongData>();
            var oldSongs = await DatabaseManager.Current.GetSongsTableFromDirectory(folder.Path);
            List<int> availableChange = new List<int>();
            List<int> availableNotChange = new List<int>();
            libraryDirectories.Add(folder.Path);

            foreach (var file in files)
            {
                string type = file.FileType.ToLower();
                if (fileFormatsHelper.IsPlaylistSupportedType(type))
                {
                    playlistFiles.Add(file);
                }
                else
                {
                    var song = oldSongs.FirstOrDefault(s => s.Path.Equals(file.Path));
                    if (null != song)
                    {
                        var prop = await file.GetBasicPropertiesAsync();
                        if (song.DateModified.Ticks == 0)
                        {
                            song.DateModified = prop.DateModified.UtcDateTime;
                            song.FileSize = (long)prop.Size;
                            if (song.IsAvailable == 1) availableNotChange.Add(song.SongId);
                            else availableChange.Add(song.SongId);
                        }
                        else if (song.DateModified < prop.DateModified.UtcDateTime)//file was modified
                        {
                            await UpdateSongMusicPropertiesAsync(song, prop, file);
                            availableChange.Add(song.SongId);
                            modifiedSongs++;
                        }
                        else
                        {
                            if (song.IsAvailable == 1) availableNotChange.Add(song.SongId);
                            else availableChange.Add(song.SongId);
                        }
                        song.IsAvailable = 1;
                    }
                    else
                    {
                        var newSong = await CreateSongFromFileAsync(file);
                        newSongs.Add(newSong);
                    }
                }
            }

            var toNotAvailable = oldSongs.Where(s => s.IsAvailable == 1 && !availableChange.Contains(s.SongId) && !availableNotChange.Contains(s.SongId)).ToList();
            foreach(var s in toNotAvailable)
            {
                s.IsAvailable = 0;
            }
            var toUpdate = oldSongs.Where(s => availableChange.Contains(s.SongId)).ToList();

            await DatabaseManager.Current.UpdateFolderAsync(folder.Path, oldSongs, newSongs, toNotAvailable, toUpdate);

            songsAdded += newSongs.Count;// + oldSongs.Where(s => s.IsAvailable == 1).Count();
            progress.Report(folder.Path + "|" + songsAdded);
            if (includeSubFolders)
            {
                var folders = await folder.GetFoldersAsync();
                foreach (var f in folders)
                {
                    await AddFilesFromFolderAsync(f);
                }
            }
        }

        public async Task MobileUpdateAsync()
        {
            var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            StorageFolder externalDevices = KnownFolders.RemovableDevices;
            StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();
            string sdCardPath = sdCard?.Path ?? "!";
            foreach (var folder in library.Folders)
            {
                if (!folder.Path.StartsWith(sdCardPath))
                {
                    await AddFilesFromFolderAsync(folder);
                }
            }
            if (sdCardFoldersToScan.Count > 0 && sdCard != null)
            {
                foreach(var folderToScan in sdCardFoldersToScan)
                {
                    try
                    {
                        var folder = await StorageFolder.GetFolderFromPathAsync(folderToScan.Path);
                        await AddFilesFromFolderAsync(folder, folderToScan.IncludeSubFolders);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public async Task ScanWholeMusicLibraryAsync()
        {
            var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            
            foreach (var folder in library.Folders)
            {
                await AddFilesFromFolderAsync(folder);
            }
        }

        private async Task UpdatePlaylistsAsync(IEnumerable<StorageFile> playlistFiles)
        {
            oldImportedPlaylists = await DatabaseManager.Current.GetImportedPlaylistsAsync();
            newImportedPlaylists = new List<ImportedPlaylist>();
            updatedPlaylists = new List<ImportedPlaylist>();
            withoutChangePlaylists = new List<ImportedPlaylist>();
            playlistsAdded = 0;

            var allSongs = await DatabaseManager.Current.GetAllSongItemsAsync();

            foreach (var file in playlistFiles)
            {
                ImportedPlaylist newPlaylist = await ImportPlaylistAsync(file, allSongs);
                var oldPlaylist = oldImportedPlaylists.FirstOrDefault(p => p.Path.Equals(file.Path));
                if (newPlaylist != null)
                {
                    var prop = await file.GetBasicPropertiesAsync();
                    DateTime dateModified = prop.DateModified.UtcDateTime;
                    if (oldPlaylist == null)
                    {
                        newImportedPlaylists.Add(newPlaylist);
                    }
                    else if (oldPlaylist.DateModified < dateModified)
                    {
                        newPlaylist.PlainPlaylistId = oldPlaylist.PlainPlaylistId;
                        updatedPlaylists.Add(newPlaylist);
                    }
                    else
                    {
                        //playlist didn't change, but still  exists
                        withoutChangePlaylists.Add(oldPlaylist);
                    }
                }
                else if (oldPlaylist != null)
                {
                    //can't read playlist now
                    //delete it
                }
            }
            foreach (var playlist in oldImportedPlaylists.Where(p => !String.IsNullOrEmpty(p.Path)))
            {
                string token = await FutureAccessHelper.GetTokenFromPathAsync(playlist.Path);
                if (token != null)
                {
                    withoutChangePlaylists.Add(playlist);
                }
            }
            var toDelete = oldImportedPlaylists.Except(withoutChangePlaylists).Except(updatedPlaylists, new ImportedPlaylistComparer()).ToList();
            playlistsAdded = newImportedPlaylists.Count;
            await DatabaseManager.Current.UpdateImportedPlaylists(toDelete, newImportedPlaylists, updatedPlaylists);
        }

        public async Task AutoUpdateDatabaseAsync(IProgress<string> p)
        {
            var freq = (TimeSpan)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LibraryUpdateFrequency);
            var updatedAt = (DateTime)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LibraryUpdatedAt);
            if (DateTime.Now > updatedAt + freq)
            {
                await UpdateDatabaseAsync(p);
            }
        }

        public async Task UpdateDatabaseAsync(IProgress<string> p)
        {
            Stopwatch s1 = new Stopwatch();
            updateDurationSeconds = 0;
            s1.Start();

            songsAdded = 0;
            modifiedSongs = 0;
            progress = p;
            libraryDirectories = new List<string>();
            playlistFiles = new List<StorageFile>();
            sdCardFoldersToScan = await ApplicationSettingsHelper.GetSdCardFoldersToScan();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.MediaScan, true);

            var propertiesToRetrieve = new List<string>();
            queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileFormatsHelper.SupportedAudioAndPlaylistFormats());
            queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
            queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.MusicProperties | PropertyPrefetchOptions.BasicProperties, propertiesToRetrieve);

            Stopwatch s = new Stopwatch();
            s.Start();
            if (DeviceFamilyHelper.IsMobile())
            {
                await MobileUpdateAsync();
            }
            else
            {
                await ScanWholeMusicLibraryAsync();
            }
            s.Stop();
            Debug.WriteLine("Scan {0}ms", s.ElapsedMilliseconds);
            
            await DatabaseManager.Current.ChangeToNotAvaialble(libraryDirectories);
            await DatabaseManager.Current.UpdateTables();

            var updated = ApplicationSettingsHelper.ReadSettingsValue("ImportPlaylistsAfterAppUpdate9");
            if (updated == null)
            {
                await ImportPlaylistsAfterAppUpdate9();
            }

            await UpdatePlaylistsAsync(playlistFiles);

            s1.Stop();
            updateDurationSeconds = s1.ElapsedMilliseconds / 1000;
            Debug.WriteLine("New songs: {0} updated songs: {1}", songsAdded, modifiedSongs);
            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LibraryUpdatedAt, DateTime.Now);
            OnMediaImported("Update");
            SendToast();
        }

        public int GetLastSongsAddedCount()
        {
            return songsAdded;
        }

        public int GetLastPlaylistsAddedCount()
        {
            return playlistsAdded;
        }

        public long GetLastUpdateDuration()
        {
            return updateDurationSeconds;
        }

        private async Task<SongData> CreateSongFromFileAsync(StorageFile file)
        {
            SongData song = new SongData();
            song.CloudUserId = "";
            song.DateAdded = DateTime.Now;
            song.Filename = file.Name;
            song.Path = file.Path;
            song.DirectoryPath = (!String.IsNullOrWhiteSpace(song.Path)) ? Path.GetDirectoryName(song.Path) : "UnknownDirectory";
            song.FolderName = Path.GetFileName(song.DirectoryPath);
            song.PlayCount = 0;
            song.LastPlayed = DateTime.MinValue;
            song.IsAvailable = 1;
            song.MusicSourceType = (int)Enums.MusicSource.LocalFile;
            song.Tag.Rating = 0;
            song.FileSize = 0;
            var prop = await file.GetBasicPropertiesAsync();
            song.DateModified = prop.DateModified.UtcDateTime;

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
                                if (!String.IsNullOrEmpty(musicProperties.Album) && musicProperties.Album != song.Tag.Album)
                                {
                                    song.Tag.Album = musicProperties.Album;
                                }
                                if (!String.IsNullOrEmpty(musicProperties.Title) && musicProperties.Title != song.Tag.Title)
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
                            song.Tag.Title = file.DisplayName ?? "";
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
                        song.Tag.Title = file.DisplayName ?? "";
                        song.Tag.Track = 0;
                        song.Tag.TrackCount = 0;
                        song.Tag.Year = 0;
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Logger2.Current.WriteMessage("CreateSongFromFile FileNotFound" + Environment.NewLine + ex.Message, Logger2.Level.Debug);
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
                song.Tag.Title = file.DisplayName ?? "";
                song.Tag.Track = 0;
                song.Tag.TrackCount = 0;
                song.Tag.Year = 0;

                return song;
            }
            catch (Exception ex)
            {
                Logger2.Current.WriteMessage("CreateSongFromFile" + Environment.NewLine + ex.Message, Logger2.Level.Debug);
                song.FileSize = 0;
            }
            if (file.DisplayName == null)
            {
                Logger2.Current.WriteMessage("CreateSongFromFile file.DisplayName == null", Logger2.Level.Information);
            }
            return song;
        }

        private async Task<ImportedPlaylist> ImportPlaylistAsync(StorageFile file, IEnumerable<SongItem> songs)
        {
            PlaylistImporter pi = new PlaylistImporter();
            ImportedPlaylist newPlaylist = await pi.Import(file);
            foreach (var m in newPlaylist.Entries)
            {
                int id = await ResolvePlaylistEntryAsync(m, songs);
                newPlaylist.SongIds.Add(id);
            }
            return newPlaylist;
        }

        /// <summary>
        /// If database doesn't contain item with entry.Path new item is created
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="allSongs"></param>
        /// <returns>songId</returns>
        private async Task<int> ResolvePlaylistEntryAsync(GeneralPlaylistEntry entry, IEnumerable<SongItem> allSongs)
        {
            var song = allSongs.FirstOrDefault(s => s.Path.Equals(entry.Path));
            int id = 0;
            if (song != null)
            {
                id = song.SongId;
            }
            else
            {
                SongData data = DatabaseManager.GetEmptySongData();
                if (entry.Path.Length>=2 && entry.Path[1] == ':')
                {
                    var file = await FutureAccessHelper.GetFileFromPathAsync(entry.Path);
                    if (file != null)
                    {
                        data = await CreateSongFromFileAsync(file);
                        id = await DatabaseManager.Current.InsertSongAsync(data);
                    }
                }
                if (id == 0)
                {
                    data.Tag.Album = entry.AlbumTitle ?? "";
                    data.Tag.AlbumArtist = entry.AlbumArtist ?? "";
                    data.Tag.Title = entry.TrackTitle ?? "";
                    data.Tag.Artists = entry.TrackArtist ?? "";
                    data.Tag.FirstArtist = entry.TrackArtist ?? "";
                    data.Duration = entry.Duration;
                    data.Path = entry.Path ?? "";
                    data.IsAvailable = 0;

                    if (data.Path.Length >= 2)
                    {
                        if (data.Path[1] == ':')//local file
                        {
                            data.MusicSourceType = (int)Enums.MusicSource.LocalNotMusicLibrary;
                            data.DirectoryPath = Path.GetDirectoryName(entry.Path);
                            data.Filename = Path.GetFileName(data.Path);
                            data.FolderName = Path.GetFileName(data.DirectoryPath);
                        }
                        else 
                        {
                            if (data.Path.Contains("://"))//stream?
                            {
                                data.Tag.Title = (data.Tag.Title != "") ? data.Tag.Title : data.Path;
                                try
                                {
                                    data.DirectoryPath = data.Path.Substring(0, data.Path.IndexOf(':') + 3);
                                }
                                catch
                                {
                                    data.DirectoryPath = data.Path;
                                }
                                data.Filename = data.Path;
                                data.FolderName = data.Path;
                                if (data.Path.StartsWith("http://") || data.Path.StartsWith("https://"))
                                {
                                    data.MusicSourceType = (int)Enums.MusicSource.OnlineFile;
                                    data.IsAvailable = 1;
                                }
                                else
                                {
                                    data.MusicSourceType = (int)Enums.MusicSource.Unknown;
                                }
                            }
                            else
                            {
                                data.DirectoryPath = data.Path;
                                data.Filename = data.Path;
                                data.FolderName = data.Path;
                                data.MusicSourceType = (int)Enums.MusicSource.Unknown;
                            }
                        }
                    }
                    else
                    {
                        data.DirectoryPath = entry.Path;
                        data.Filename = entry.Path;
                        data.FolderName = entry.Path;
                        data.MusicSourceType = (int)Enums.MusicSource.Unknown;
                    }
                    id = await DatabaseManager.Current.InsertSongAsync(data);
                }
            }
            return id;
        }

        public async Task<SongItem> OpenSingleFileAsync(StorageFile file)
        {
            string type = file.FileType.ToLower();
            SongItem song = new SongItem();
            if (fileFormatsHelper.IsFormatSupported(type))
            {
                song = await DatabaseManager.Current.GetSongItemIfExistAsync(file.Path);
                if (song == null)
                {
                    await FutureAccessHelper.AddToFutureAccessListAndSaveTokenAsync(file);
                    var songData = await CreateSongFromFileAsync(file);
                    await ImagesManager.SaveAlbumArtFromSong(songData);
                    songData.IsAvailable = 0;
                    int id = await DatabaseManager.Current.InsertSongAsync(songData);
                    song = await DatabaseManager.Current.GetSongItemAsync(id);
                }
            }
            return song;
        }

        public async Task<PlaylistItem> OpenPlaylistFileAsync(StorageFile file)
        {
            string type = file.FileType.ToLower();
            if (fileFormatsHelper.IsPlaylistSupportedType(type))
            {
                var prop = await file.GetBasicPropertiesAsync();
                var playlists = await DatabaseManager.Current.GetPlainPlaylistsAsync();
                var playlistItem = playlists.FirstOrDefault(p => p.Path.Equals(file.Path));
                if (playlistItem == null || playlistItem.DateModified < prop.DateModified)
                {
                    var allSongs = await DatabaseManager.Current.GetAllSongItemsAsync();
                    ImportedPlaylist playlist = await ImportPlaylistAsync(file, allSongs);
                    if (playlist!= null)
                    {
                        int id = await DatabaseManager.Current.InsertOrUpdateImportedPlaylist(playlist);
                        playlistItem = await DatabaseManager.Current.GetPlainPlaylistAsync(id);
                        await FutureAccessHelper.AddToFutureAccessListAndSaveTokenAsync(file);
                    }
                }
                return playlistItem;
            }
            return null;
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

        public static async Task UpdateCacheTablesAsync()
        {
            await DatabaseManager.Current.UpdateTables();

        }

        private async Task ImportPlaylistsAfterAppUpdate9()
        {           
            PlaylistHelper ph = new PlaylistHelper();
            var allSongs = await DatabaseManager.Current.GetAllSongItemsAsync();
            var folder = await ph.GetFolderWithAppPlaylistsAsync();
            queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new List<string> { ".m3u" });
            queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
            var query = folder.CreateFileQueryWithOptions(queryOptions);
            var files = await query.GetFilesAsync();
            foreach(var file in files)
            {
                ImportedPlaylist newPlaylist = await ImportPlaylistAsync(file, allSongs);
                int id = DatabaseManager.Current.InsertPlainPlaylist(newPlaylist.Name);
                await DatabaseManager.Current.AddToPlaylist(id, newPlaylist.SongIds);
            }
            ApplicationSettingsHelper.SaveSettingsValue("ImportPlaylistsAfterAppUpdate9", true);
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
    }
}
