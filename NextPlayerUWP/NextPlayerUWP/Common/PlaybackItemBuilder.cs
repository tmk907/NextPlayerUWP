using FFmpegInterop;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
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
            string token = await FutureAccessHelper.GetTokenFromPath(path);
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
                    if (App.AudioFormatsHelper.IsDefaultSupportedType(song.Path.Substring(song.Path.LastIndexOf('.'))))
                    {
                        mpi = PrepareFromLocalFile(song);
                    }
                    else
                    {
                        mpi = null;// await PrepareFromLocalFileFFmpeg(song);
                    }
                    break;
                case MusicSource.LocalNotMusicLibrary:
                    if (App.AudioFormatsHelper.IsDefaultSupportedType(song.Path.Substring(song.Path.LastIndexOf('.'))))
                    {
                        mpi = PrepareFromFutureAccessList(song);
                    }
                    else
                    {
                        mpi = null;// await PrepareFromFutureAccessListFFmpeg(song);
                    }
                    break;
                case MusicSource.RadioJamendo:
                    mpi = await PrepareFromJamendo(song);
                    break;
                case MusicSource.Dropbox:
                    mpi = PrepareFromCloudStorage(song);
                    break;
                case MusicSource.OneDrive:
                    mpi = PrepareFromCloudStorage(song);
                    break;
                case MusicSource.PCloud:
                    mpi = PrepareFromCloudStorage(song);
                    break;
                default:
                    mpi = null;
                    break;
            }
            return mpi;
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

        private static async Task<MediaPlaybackItem> PrepareFromLocalFileFFmpeg(SongItem song)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
            IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);

            try
            {
                // Instantiate FFmpegInteropMSS using the opened local file stream
                FFmpegInteropMSS FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, true, false);
                MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                if (mss != null)
                {
                    
                    var source = MediaSource.CreateFromMediaStreamSource(mss);
                    //StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                    //var source = MediaSource.CreateFromStorageFile(file);
                    var playbackItem = new MediaPlaybackItem(source);
                    playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
                    UpdateDisplayProperties(playbackItem, song);
                    source.CustomProperties[propertySongId] = song.SongId;
                    return playbackItem;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

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

        private static async Task<MediaPlaybackItem> PrepareFromFutureAccessListFFmpeg(SongItem song)
        {
            string token = await FutureAccessHelper.GetTokenFromPath(song.Path);
            if (token != null)
            {
                StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(song.Path);
                IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);

                try
                {
                    // Instantiate FFmpegInteropMSS using the opened local file stream
                    FFmpegInteropMSS FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, true, false);
                    MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                    if (mss != null)
                    {
                        var source = MediaSource.CreateFromMediaStreamSource(mss);
                        //StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                        //var source = MediaSource.CreateFromStorageFile(file);
                        var playbackItem = new MediaPlaybackItem(source);
                        playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
                        UpdateDisplayProperties(playbackItem, song);
                        source.CustomProperties[propertySongId] = song.SongId;
                        return playbackItem;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return null;
        }

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

        private static async void BindSource(MediaBinder sender, MediaBindingEventArgs args)
        {
            var deferral = args.GetDeferral();
            int id = Int32.Parse(args.MediaBinder.Token);
            var song = NowPlayingPlaylistManager.Current.songs.FirstOrDefault(s => s.SongId == id);
            await UpdateSongContentPath(song, song.SourceType);
            args.SetUri(new Uri(song.ContentPath));

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
            var displayProperties = playbackItem.GetDisplayProperties();
            displayProperties.Type = Windows.Media.MediaPlaybackType.Music;
            displayProperties.MusicProperties.Artist = song.Artist;
            displayProperties.MusicProperties.AlbumTitle = song.Album;
            displayProperties.MusicProperties.Title = song.Title;
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
