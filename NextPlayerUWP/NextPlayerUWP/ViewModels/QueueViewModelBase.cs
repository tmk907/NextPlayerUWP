using Microsoft.Toolkit.Uwp;
using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;

namespace NextPlayerUWP.ViewModels
{
    public class QueueViewModelBase : Template10.Mvvm.BindableBase
    {
        public QueueViewModelBase()
        {
            System.Diagnostics.Debug.WriteLine("QueueViewModelBase");
            isInitialized = false;
            Init();
            App.Current.EnteredBackground += Current_EnteredBackground;
            App.Current.LeavingBackground += Current_LeavingBackground;
            lastFmCache = new LastFmCache();
            accentToken = MessageHub.Instance.Subscribe<AppColorAccent>(OnAppAccentChange);
        }
        private LastFmCache lastFmCache;
        private bool isInitialized;
        private Guid accentToken;

        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            Init();
        }

        private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            PlaybackService.MediaPlayerTrackChanged -= ChangeSong;
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            NowPlayingPlaylistManager.NPListChanged -= UpdatePlaylist;
            isInitialized = false;
        }

        private void Init()
        {
            if (isInitialized) return;
            System.Diagnostics.Debug.WriteLine("QueueViewModelBase.Init()");
            CurrentSong = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (!currentSong.IsAlbumArtSet)
            {
                CoverUri = SongCoverManager.Instance.GetCurrent();
            }
            else
            {
                CoverUri = currentSong.AlbumArtUri;
            }
            AlbumArtColor = Colors.Gray;
            //ChangeAlbumArtColor();
            if (songs.Count == 0)
            {
                UpdatePlaylist();
            }
            CurrentSongNumber = PlaybackService.Instance.CurrentSongIndex + 1;
            PlaybackService.MediaPlayerTrackChanged += ChangeSong;
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            NowPlayingPlaylistManager.NPListChanged += UpdatePlaylist;
            isInitialized = true;
        }

        private SongItem currentSong = new SongItem();
        public SongItem CurrentSong
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    currentSong = new SongItem();
                }
                return currentSong;
            }
            set { Set(ref currentSong, value); }
        }

        private int currentSongNumber = 0;
        public int CurrentSongNumber
        {
            get { return currentSongNumber; }
            set { Set(ref currentSongNumber, value); }
        }

        private int songsCount = 0;
        public int SongsCount
        {
            get { return songsCount; }
            set { Set(ref songsCount, value); }
        }

        private Uri coverUri;
        public Uri CoverUri
        {
            get { return coverUri; }
            set
            {
                Set(ref coverUri, value);
            }
        }

        private bool useAlbumArtAccent = false;
        private ColorsHelper colorsHelper = new ColorsHelper();
        private ConcurrentDictionary<Uri, Color> cachedColors = new ConcurrentDictionary<Uri, Color>();

        private Color albumArtColor;
        public Color AlbumArtColor
        {
            get { return albumArtColor; }
            set { Set(ref albumArtColor, value); }
        }

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        public async Task RateSong(int rating)
        {
            if (CurrentSong.SourceType == MusicSource.LocalFile || CurrentSong.SourceType == MusicSource.LocalNotMusicLibrary)
            {
                CurrentSong.Rating = rating;
                await lastFmCache.RateSong(CurrentSong.Artist, CurrentSong.Title, rating);
                await DatabaseManager.Current.UpdateRatingAsync(CurrentSong.SongId, CurrentSong.Rating).ConfigureAwait(false);
            }
        }

        private async void ChangeSong(int index)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                CurrentSong = NowPlayingPlaylistManager.Current.GetSongItem(index);
                CoverUri = currentSong.AlbumArtUri;
                CurrentSongNumber = index + 1;
            });
            await ChangeAlbumArtColor();
        }

        public async void ChangeCoverUri(Uri cacheUri)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                CoverUri = cacheUri;
            });
            await ChangeAlbumArtColor();
        }

        private void UpdatePlaylist()
        {
            Songs = NowPlayingPlaylistManager.Current.songs;
            SongsCount = NowPlayingPlaylistManager.Current.songs.Count;
        }

        private async void PlaybackService_MediaPlayerMediaOpened()
        {
            await Task.Delay(400);
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                var duration = PlaybackService.Instance.Duration;
                if (duration == TimeSpan.MaxValue)
                {
                    duration = TimeSpan.Zero;
                }
                if (currentSong.Duration == TimeSpan.Zero && 
                currentSong.SourceType == MusicSource.LocalFile || 
                currentSong.SourceType == MusicSource.Dropbox ||
                currentSong.SourceType == MusicSource.OneDrive || 
                currentSong.SourceType == MusicSource.PCloud)
                {
                    currentSong.Duration = duration;
                    await DatabaseManager.Current.UpdateSongDurationAsync(currentSong.SongId, currentSong.Duration);//.ConfigureAwait(false);
                }
            });
        }

        private void OnAppAccentChange(AppColorAccent message)
        {
            useAlbumArtAccent = message.UseAlbumArtAccent;
            if (useAlbumArtAccent)
            {
                ChangeAlbumArtColor();
            }
        }

        private async Task ChangeAlbumArtColor()
        {
            if (useAlbumArtAccent)
            {
                if (cachedColors.ContainsKey(coverUri))
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        AlbumArtColor = cachedColors[coverUri];
                        colorsHelper.ChangeAppAccentColor(albumArtColor);
                    });
                }
                else
                {
                    var color = await colorsHelper.GetDominantColorFromSavedAlbumArt(coverUri);
                    cachedColors.TryAdd(coverUri, color);
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        AlbumArtColor = color;
                        colorsHelper.ChangeAppAccentColor(albumArtColor);
                    });
                }
            }
        }
    }
}
