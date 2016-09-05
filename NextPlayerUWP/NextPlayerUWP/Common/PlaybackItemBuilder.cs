using FFmpegInterop;
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

        private static bool IsTypeDefaultSupported(string type)
        {
            return (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                    type == ".wav" || type == ".aac" || type == ".asf" || type == ".flac" ||
                    type == ".adt" || type == ".adts" || type == ".amr" || type == ".mp4");
        }

        public static async Task<MediaPlaybackItem> PreparePlaybackItem(SongItem song)
        {
            MediaPlaybackItem mpi;

            switch (song.SourceType)
            {
                case MusicSource.LocalFile:
                    if (IsTypeDefaultSupported(song.Path.Substring(song.Path.LastIndexOf('.'))))
                    {
                        mpi = PrepareFromLocalFile(song);
                    }
                    else
                    {
                        mpi = await PrepareFromLocalFileFFmpeg(song);
                    }
                    break;
                case MusicSource.LocalNotMusicLibrary:
                    if (IsTypeDefaultSupported(song.Path.Substring(song.Path.LastIndexOf('.'))))
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
                    mpi = PrepareFromDropbox(song);
                    break;
                case MusicSource.OneDrive:
                    mpi = PrepareFromOneDrive(song);
                    break;
                default:
                    mpi = null;
                    break;
            }
            return mpi;
        }

        private static MediaPlaybackItem PrepareFromLocalFile(SongItem song)
        {
            MyStreamReference msr = new MyStreamReference(song.Path);
            var source = MediaSource.CreateFromStreamReference(msr, "");
            //StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
            //var source = MediaSource.CreateFromStorageFile(file);
            var playbackItem = new MediaPlaybackItem(source);
            playbackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
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
            var source2 = MediaSource.CreateFromStreamReference(msrfal, "");
            var playbackItem2 = new MediaPlaybackItem(source2);
            playbackItem2.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            var displayProperties2 = playbackItem2.GetDisplayProperties();
            displayProperties2.Type = Windows.Media.MediaPlaybackType.Music;
            displayProperties2.MusicProperties.Artist = song.Artist;
            displayProperties2.MusicProperties.AlbumTitle = song.Album;
            displayProperties2.MusicProperties.Title = song.Title;
            try
            {
                displayProperties2.Thumbnail = RandomAccessStreamReference.CreateFromUri(song.AlbumArtUri);
            }
            catch
            {
                displayProperties2.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
            }
            playbackItem2.ApplyDisplayProperties(displayProperties2);
            source2.CustomProperties[propertySongId] = song.SongId;
            return playbackItem2;
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
            var jamendoDisplayProperties = jamendoPlaybackItem.GetDisplayProperties();
            jamendoDisplayProperties.Type = Windows.Media.MediaPlaybackType.Music;
            jamendoDisplayProperties.MusicProperties.Artist = song.Artist;
            jamendoDisplayProperties.MusicProperties.AlbumTitle = song.Album;
            jamendoDisplayProperties.MusicProperties.Title = song.Title;
            try
            {
                jamendoDisplayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(song.AlbumArtUri);
            }
            catch
            {
                jamendoDisplayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
            }
            jamendoPlaybackItem.ApplyDisplayProperties(jamendoDisplayProperties);
            jamendoSource.CustomProperties[propertySongId] = song.SongId;
            return jamendoPlaybackItem;
        }

        private static MediaPlaybackItem PrepareFromDropbox(SongItem song)
        {
            var dropboxSource = MediaSource.CreateFromUri(new Uri(song.Path));
            var dropboxPlaybackItem = new MediaPlaybackItem(dropboxSource);
            dropboxPlaybackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            var dropboxDisplayProperties = dropboxPlaybackItem.GetDisplayProperties();
            dropboxDisplayProperties.Type = Windows.Media.MediaPlaybackType.Music;
            dropboxDisplayProperties.MusicProperties.Artist = song.Artist;
            dropboxDisplayProperties.MusicProperties.AlbumTitle = song.Album;
            dropboxDisplayProperties.MusicProperties.Title = song.Title;
            try
            {
                dropboxDisplayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(song.AlbumArtUri);
            }
            catch
            {
                dropboxDisplayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
            }
            dropboxPlaybackItem.ApplyDisplayProperties(dropboxDisplayProperties);
            dropboxSource.CustomProperties[propertySongId] = song.SongId;
            return dropboxPlaybackItem;
        }

        private static MediaPlaybackItem PrepareFromOneDrive(SongItem song)
        {
            var oneDriveSource = MediaSource.CreateFromUri(new Uri(song.Path));
            var oneDrivePlaybackItem = new MediaPlaybackItem(oneDriveSource);
            oneDrivePlaybackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
            var oneDriveDisplayProperties = oneDrivePlaybackItem.GetDisplayProperties();
            oneDriveDisplayProperties.Type = Windows.Media.MediaPlaybackType.Music;
            oneDriveDisplayProperties.MusicProperties.Artist = song.Artist;
            oneDriveDisplayProperties.MusicProperties.AlbumTitle = song.Album;
            oneDriveDisplayProperties.MusicProperties.Title = song.Title;
            try
            {
                oneDriveDisplayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(song.AlbumArtUri);
            }
            catch
            {
                oneDriveDisplayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
            }
            oneDrivePlaybackItem.ApplyDisplayProperties(oneDriveDisplayProperties);
            oneDriveSource.CustomProperties[propertySongId] = song.SongId;
            return oneDrivePlaybackItem;
        }
    }
}
