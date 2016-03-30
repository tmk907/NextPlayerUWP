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
using TagLib;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Notifications;

namespace NextPlayerUWPDataLayer.Services
{
    public delegate void MediaImportedHandler(string s);

    public class MediaImport
    {
        public static event MediaImportedHandler MediaImported;

        public static void OnMediaImported(string s)
        {
            if (MediaImported != null)
            {
                MediaImported(s);
            }
        }

        private Dictionary<string, Tuple<int, int>> dbFiles;
        private int songsAdded;
        private IProgress<int> progress;

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
            //.mp4
            //.wm
            //.3gpp
            //.3gp2
            //.mpa
            //.pya
            foreach (var file in files)
            {
                string type = file.FileType.ToLower();
                if (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                    type == ".wav" || type == ".aac" || type == ".asf" || type == ".flac" ||
                    type == ".adt" || type == ".adts" || type == ".amr" || type == ".mp4")
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

            if (newSongs.Count == 0 && toAvailable.Count == 0 && oldAvailable.Count > 0)
            {

            }
            else
            {
                await DatabaseManager.Current.UpdateFolderAsync(newSongs, toAvailable, oldAvailable, folder.Path);
            }
            songsAdded += newSongs.Count;
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
            var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.MediaScan, true);
            foreach (var folder in library.Folders)
            {
                await AddFilesFromFolder(folder);
            }

            await DatabaseManager.Current.UpdateTables();
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

        private void SendToast()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(loader.GetString("LibraryUpdated")));
            ToastNotification toast = new ToastNotification(toastXml);
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
    }
}
