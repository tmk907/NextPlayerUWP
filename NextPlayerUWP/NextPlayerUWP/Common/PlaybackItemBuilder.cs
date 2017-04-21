//using FFmpegInterop;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Playlists;
using PlaylistsNET.Content;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage;
using Windows.Storage.Streams;

namespace NextPlayerUWP.Common
{
    public class MyStreamReference : IRandomAccessStreamReference
    {
        private string path;

        public MyStreamReference(string path)
        {
            this.path = path;
        }

        // private async helper task that is necessary if you need to use await.
        private async Task<IRandomAccessStreamWithContentType> Open()
            => await (await StorageFile.GetFileFromPathAsync(path)).OpenReadAsync();

        IAsyncOperation<IRandomAccessStreamWithContentType> IRandomAccessStreamReference.OpenReadAsync()
        {
            return Open().AsAsyncOperation();
        }
    }

    public class MyStreamReferenceFAL : IRandomAccessStreamReference
    {
        private string path;

        public MyStreamReferenceFAL(string path)
        {
            this.path = path;
        }

        // private async helper task that is necessary if you need to use await.
        private async Task<IRandomAccessStreamWithContentType> Open()
        {
            string token = await FutureAccessHelper.GetTokenFromPathAsync(path);
            if (token != null)
            {
                var file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                return await file.OpenReadAsync();
            }
            return null;
        }
        //=> await (await StorageFile.GetFileFromPathAsync(path)).OpenReadAsync();

        IAsyncOperation<IRandomAccessStreamWithContentType> IRandomAccessStreamReference.OpenReadAsync()
        {
            return Open().AsAsyncOperation();
        }
    }

    public class PlaybackItemBuilder
    {
        private const string propertyIndex = "index";
        private const string propertySongId = "songid";

        public static async Task<MediaPlaybackItem> PreparePlaybackItem(SongItem song)
        {
            MediaPlaybackItem mpi;

            switch (song.SourceType)
            {
                case MusicSource.LocalFile:
                    if (App.FileFormatsHelper.IsDefaultSupportedType(song.Path.GetExtension()))
                    {
                        mpi = PrepareFromLocalFile(song);
                    }
                    else
                    {
                        mpi = PrepareDefaultItem();// await PrepareFromLocalFileFFmpeg(song);
                    }
                    break;
                case MusicSource.LocalNotMusicLibrary:
                    if (App.FileFormatsHelper.IsDefaultSupportedType(song.Path.GetExtension()))
                    {
                        mpi = PrepareFromFutureAccessList(song);
                    }
                    else
                    {
                        mpi = PrepareDefaultItem();// await PrepareFromFutureAccessListFFmpeg(song);
                    }
                    break;
                case MusicSource.RadioJamendo:
                    mpi = await PrepareFromJamendo(song);
                    break;
                case MusicSource.Dropbox:
                case MusicSource.OneDrive:
                case MusicSource.PCloud:
                    mpi = PrepareFromCloudStorage(song);
                    break;
                case MusicSource.Radio:
                case MusicSource.OnlineFile:
                    mpi = PrepareFromOnlineFile(song);
                    break;
                default:
                    mpi = PrepareDefaultItem();
                    break;
            }
            return mpi;
        }

        public static MediaPlaybackItem PrepareDefaultItem()
        {
            //MyStreamReference msr = new MyStreamReference(AppConstants.EmptyMP3File);
            var source = MediaSource.CreateFromUri(new Uri(AppConstants.EmptyMP3File));//.CreateFromStreamReference(msr, "");
            var playbackItem = new MediaPlaybackItem(source);
            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(playbackItem, new SongItem());
            source.CustomProperties[propertySongId] = -1;
            return playbackItem;
        }

        public static MediaPlaybackItem PrepareFromLocalFile(SongItem song)
        {
            MyStreamReference msr = new MyStreamReference(song.ContentPath);
            var source = MediaSource.CreateFromStreamReference(msr, "");
            var playbackItem = new MediaPlaybackItem(source);
            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(playbackItem, song);
            source.CustomProperties[propertySongId] = song.SongId;
            return playbackItem;
        }

        //private static async Task<MediaPlaybackItem> PrepareFromLocalFileFFmpeg(SongItem song)
        //{
        //    StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
        //    IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);

        //    try
        //    {
        //        // Instantiate FFmpegInteropMSS using the opened local file stream
        //        FFmpegInteropMSS FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, true, false);
        //        MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

        //        if (mss != null)
        //        {
                    
        //            var source = MediaSource.CreateFromMediaStreamSource(mss);
        //            //StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
        //            //var source = MediaSource.CreateFromStorageFile(file);
        //            var playbackItem = new MediaPlaybackItem(source);
        //            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
        //            UpdateDisplayProperties(playbackItem, song);
        //            source.CustomProperties[propertySongId] = song.SongId;
        //            return playbackItem;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return null;
        //}

        private static MediaPlaybackItem PrepareFromFutureAccessList(SongItem song)
        {
            MyStreamReferenceFAL msrfal = new MyStreamReferenceFAL(song.Path);
            var source = MediaSource.CreateFromStreamReference(msrfal, "");
            var playbackItem = new MediaPlaybackItem(source);
            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(playbackItem, song);
            source.CustomProperties[propertySongId] = song.SongId;
            return playbackItem;
        }

        //private static async Task<MediaPlaybackItem> PrepareFromFutureAccessListFFmpeg(SongItem song)
        //{
        //    string token = await FutureAccessHelper.GetTokenFromPath(song.Path);
        //    if (token != null)
        //    {
        //        StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(song.Path);
        //        IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);

        //        try
        //        {
        //            // Instantiate FFmpegInteropMSS using the opened local file stream
        //            FFmpegInteropMSS FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, true, false);
        //            MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

        //            if (mss != null)
        //            {
        //                var source = MediaSource.CreateFromMediaStreamSource(mss);
        //                //StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
        //                //var source = MediaSource.CreateFromStorageFile(file);
        //                var playbackItem = new MediaPlaybackItem(source);
        //                playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
        //                UpdateDisplayProperties(playbackItem, song);
        //                source.CustomProperties[propertySongId] = song.SongId;
        //                return playbackItem;
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    return null;
        //}

        private static async Task<MediaPlaybackItem> PrepareFromJamendo(SongItem song)
        {
            if ("" == song.Path)
            {
                var stream = await PlaybackService.Instance.jRadioData.GetRadioStream(song.SongId);
                if (stream != null)
                {
                    song.Path = stream.Url;
                }
            }
            var jamendoSource = MediaSource.CreateFromUri(new Uri(song.Path));
            var jamendoPlaybackItem = new MediaPlaybackItem(jamendoSource);
            jamendoPlaybackItem.Source.OpenOperationCompleted += PlaybackService.RadioSource_OpenOperationCompleted;
            UpdateDisplayProperties(jamendoPlaybackItem, song);
            jamendoSource.CustomProperties[propertySongId] = song.SongId;
            return jamendoPlaybackItem;
        }

        private static MediaPlaybackItem PrepareFromCloudStorage(SongItem song)
        {
            MediaBinder mb = new MediaBinder();
            mb.Token = song.SongId.ToString();
            mb.Binding += BindSource;
               
            var source = MediaSource.CreateFromMediaBinder(mb);
            source.CustomProperties[propertySongId] = song.SongId;
            var playbackItem = new MediaPlaybackItem(source);
            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(playbackItem, song);
            
            return playbackItem;
        }

        private static MediaPlaybackItem PrepareFromOnlinePlaylist(SongItem song)
        {
            MediaBinder mb = new MediaBinder();
            mb.Token = song.SongId.ToString();
            mb.Binding += BindSourcePlaylist;

            var source = MediaSource.CreateFromMediaBinder(mb);
            source.CustomProperties[propertySongId] = song.SongId;
            var playbackItem = new MediaPlaybackItem(source);
            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(playbackItem, song);

            return playbackItem;
        }

        private static async void BindSourcePlaylist(MediaBinder sender, MediaBindingEventArgs args)
        {
            var deferral = args.GetDeferral();
            int id = Int32.Parse(args.MediaBinder.Token);
            var song = NowPlayingPlaylistManager.Current.songs.FirstOrDefault(s => s.SongId == id);
            try
            {
                using (var request = new Microsoft.Toolkit.Uwp.HttpHelperRequest(new Uri(song.Path), Windows.Web.Http.HttpMethod.Get))
                {
                    using (var response = await Microsoft.Toolkit.Uwp.HttpHelper.Instance.SendRequestAsync(request))
                    {
                        string p = await response.GetTextResultAsync();
                        PlaylistFileReader pfr = new PlaylistFileReader();
                        if (song.Path.EndsWith(".m3u"))
                        {
                            var m3u = pfr.OpenM3uPlaylist(p);
                            if (m3u.PlaylistEntries.Count > 0)
                            {
                                args.SetUri(new Uri(m3u.PlaylistEntries[0].Path));
                            }
                            else
                            {
                                args.SetUri(new Uri(AppConstants.EmptyMP3File));
                            }
                        }
                        else if (song.Path.EndsWith(".pls"))
                        {
                            var pls = pfr.OpenPlsPlaylist(p);
                            if (pls.PlaylistEntries.Count > 0)
                            {
                                args.SetUri(new Uri(pls.PlaylistEntries[0].Path));
                            }
                            else
                            {
                                args.SetUri(new Uri(AppConstants.EmptyMP3File));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                args.SetUri(new Uri(AppConstants.EmptyMP3File));
            }
            deferral.Complete();
        }

        private static MediaPlaybackItem PrepareFromOnlineFile(SongItem song)
        {
            Uri uri;
            try
            {
                if (song.Path.EndsWith(".pls") || song.Path.EndsWith(".m3u"))
                {
                    return PrepareFromOnlinePlaylist(song);
                }
                uri = new Uri(song.Path);
            }
            catch (UriFormatException ex)
            {
                return PrepareDefaultItem();
            }
            var source = MediaSource.CreateFromUri(uri);
            var playbackItem = new MediaPlaybackItem(source);
            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(playbackItem, song);
            source.CustomProperties[propertySongId] = song.SongId;
            return playbackItem;
        }

        private static async void BindSource(MediaBinder sender, MediaBindingEventArgs args)
        {
            var deferral = args.GetDeferral();
            int id = Int32.Parse(args.MediaBinder.Token);
            var song = NowPlayingPlaylistManager.Current.songs.FirstOrDefault(s => s.SongId == id);
            await UpdateSongContentPath(song, song.SourceType);
            if (String.IsNullOrEmpty(song.ContentPath))
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("BindSource path error", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                TelemetryAdapter.TrackEvent("BindSource path error");
                args.SetUri(new Uri(AppConstants.EmptyMP3File));
            }
            else
            {
                args.SetUri(new Uri(song.ContentPath));
            }

            deferral.Complete();
        }

        private static async Task UpdateSongContentPath(SongItem song, MusicSource type)
        {
            if (song.IsContentPathExpired())
            {
                CloudStorageType cloudType;
                switch (type)
                {
                    case MusicSource.Dropbox:
                        cloudType = CloudStorageType.Dropbox;
                        break;
                    case MusicSource.GoogleDrive:
                        cloudType = CloudStorageType.GoogleDrive;
                        break;
                    case MusicSource.OneDrive:
                        cloudType = CloudStorageType.OneDrive;
                        break;
                    case MusicSource.PCloud:
                        cloudType = CloudStorageType.pCloud;
                        break;
                    default:
                        return;
                }
                CloudStorageServiceFactory cssf = new CloudStorageServiceFactory();
                var service = cssf.GetService(cloudType, song.CloudUserId);
                try
                {
                    var link = await service.GetDownloadLink(song.Path);
                    var validForHours = (cloudType == CloudStorageType.OneDrive) ? 100 : (cloudType == CloudStorageType.Dropbox || cloudType == CloudStorageType.pCloud) ? 4 : 1;
                    song.UpdateContentPath(link, DateTime.Now.AddHours(validForHours));
                }
                catch (Exception ex)
                {
                    song.UpdateContentPath("", DateTime.Now.AddSeconds(10));
                }
            }
        }

        private static void UpdateDisplayProperties(MediaPlaybackItem playbackItem, SongItem song)
        {
            if (playbackItem == null) return;//TODO catch error null reference?
            var displayProperties = playbackItem.GetDisplayProperties();
            displayProperties.Type = Windows.Media.MediaPlaybackType.Music;
            displayProperties.MusicProperties.Artist = song.Artist ?? ""; // is it neccesarry?
            displayProperties.MusicProperties.AlbumTitle = song.Album ?? "";
            displayProperties.MusicProperties.Title = song.Title ?? "";
            try
            {
                displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(song.AlbumArtUri);
            }
            catch
            {
                displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
            }
            playbackItem.ApplyDisplayProperties(displayProperties);
        }
    }
}
