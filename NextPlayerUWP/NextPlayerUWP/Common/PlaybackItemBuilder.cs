using FFmpegInterop;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.CloudStorage.DropboxStorage;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace NextPlayerUWP.Common
{
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
                    if (PlaybackService.IsTypeDefaultSupported(song.Path.Substring(song.Path.LastIndexOf('.'))))
                    {
                        mpi = PrepareFromLocalFile(song);
                    }
                    else
                    {
                        mpi = await PrepareFromLocalFileFFmpeg(song);
                    }
                    break;
                case MusicSource.LocalNotMusicLibrary:
                    if (PlaybackService.IsTypeDefaultSupported(song.Path.Substring(song.Path.LastIndexOf('.'))))
                    {
                        mpi = PrepareFromFutureAccessList(song);
                    }
                    else
                    {
                        mpi = await PrepareFromFutureAccessListFFmpeg(song);
                    }
                    break;
                case MusicSource.RadioJamendo:
                    mpi = await PrepareFromJamendo(song);
                    break;
                case MusicSource.Dropbox:
                    mpi = await PrepareFromDropbox(song);
                    break;
                case MusicSource.OneDrive:
                    mpi = await PrepareFromOneDrive(song);
                    break;
                default:
                    mpi = null;
                    break;
            }
            return mpi;
        }

        private static MediaPlaybackItem PrepareFromLocalFile(SongItem song)
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

        private static async Task<MediaPlaybackItem> PrepareFromDropbox(SongItem song)
        {
            if (song.IsContentPathExpired())
            {
                CloudStorageServiceFactory cssf = new CloudStorageServiceFactory();
                var service = cssf.GetService(CloudStorageType.Dropbox, song.CloudUserId);
                var link = await service.GetDownloadLink(song.Path);
                song.UpdateContentPath(link, DateTime.Now.AddHours(4));
            }
            var dropboxSource = MediaSource.CreateFromUri(new Uri(song.ContentPath));
            var dropboxPlaybackItem = new MediaPlaybackItem(dropboxSource);
            dropboxPlaybackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(dropboxPlaybackItem, song);
            dropboxSource.CustomProperties[propertySongId] = song.SongId;
            return dropboxPlaybackItem;
        }

        private static async Task<MediaPlaybackItem> PrepareFromOneDrive(SongItem song)
        {
            if (song.IsContentPathExpired())
            {
                CloudStorageServiceFactory cssf = new CloudStorageServiceFactory();
                var service = cssf.GetService(CloudStorageType.OneDrive, song.CloudUserId);
                var link = await service.GetDownloadLink(song.Path);
                song.UpdateContentPath(link, DateTime.Now.AddHours(100));
            }
            var oneDriveSource = MediaSource.CreateFromUri(new Uri(song.ContentPath));
            var oneDrivePlaybackItem = new MediaPlaybackItem(oneDriveSource);
            oneDrivePlaybackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            UpdateDisplayProperties(oneDrivePlaybackItem, song);
            oneDriveSource.CustomProperties[propertySongId] = song.SongId;
            return oneDrivePlaybackItem;
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
