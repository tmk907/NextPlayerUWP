using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Images.PaletteUWP;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Services
{
    public static class Abc
    {
        public static async Task RunWithMaxDegreeOfConcurrency<T>(
            int maxDegreeOfConcurrency, IEnumerable<T> collection, Func<T, Task> taskFactory)
        {
            var activeTasks = new List<Task>(maxDegreeOfConcurrency);
            foreach (var task in collection.Select(taskFactory))
            {
                activeTasks.Add(task);
                if (activeTasks.Count == maxDegreeOfConcurrency)
                {
                    await Task.WhenAny(activeTasks.ToArray());
                    //observe exceptions here
                    activeTasks.RemoveAll(t => t.IsCompleted);
                }
            }
            await Task.WhenAll(activeTasks.ToArray()).ContinueWith(t =>
            {
                //observe exceptions in a manner consistent with the above   
            });
        }
    }

    public class MigrationHelper
    {
        public async Task MigrateAlbumArts()
        {
            //var migrated = ApplicationSettingsHelper.ReadSettingsValue<bool>("MigrateAlbumArts1");
            //if (migrated) return;
            PaletteHelper ph = new PaletteHelper();

            var songs = await DatabaseManager.Current.GetSongsTableAsync();
            var notAvailable = songs.Where(so => so.IsAvailable == 0).ToList();
            await DatabaseManager.Current.UpdateSongsTableAsync(notAvailable);
            songs = songs.Where(so => so.IsAvailable > 0 && so.AlbumArt.StartsWith("ms-appdata:///local/Songs/")).ToList();

            StorageFolder songsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Songs");
            StorageFolder albumArts = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArts", CreationCollisionOption.OpenIfExists);
            var files = await songsFolder.GetFilesAsync();

            foreach (var song in songs)
            {
                var c = await ph.GetColorFromLocalAlbumArt(new Uri(song.AlbumArt));
                int i = ColorHelpers.ColorToInt(c);
                var sd = new SongData();
                sd.Tag.Album = song.Album;
                sd.Tag.Artists = song.Artists;
                sd.Tag.Title = song.Title;
                var filename = AlbumArtsManager.GetFileName(sd, i);
                var file = files.FirstOrDefault(f => f.Name.Equals(song.AlbumArt.Substring(26)));
                if (file != null)
                {
                    try
                    {
                        await file.MoveAsync(albumArts, filename + ".jpg", NameCollisionOption.FailIfExists);
                        song.AlbumArt = "ms-appdata:///local/AlbumArts/" + filename + ".jpg";
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        try
                        {
                            await file.CopyAsync(albumArts, filename + ".jpg", NameCollisionOption.FailIfExists);
                            song.AlbumArt = "ms-appdata:///local/AlbumArts/" + filename + ".jpg";
                        }
                        catch(Exception ex2)
                        {

                        }
                    }
                    catch (Exception ex3)
                    {
                        song.AlbumArt = "ms-appdata:///local/AlbumArts/" + filename + ".jpg";
                    }
                }
            }

            await DatabaseManager.Current.UpdateSongsTableAsync(songs);

            ApplicationSettingsHelper.SaveSettingsValue("MigrateAlbumArts1", true);
        }
    }
}
