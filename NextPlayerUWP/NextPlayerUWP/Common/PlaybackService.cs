using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Jamendo;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.System.Threading;

namespace NextPlayerUWP.Common
{
    public delegate void MediaPlayerStateChangeHandler(MediaPlaybackState state);
    public delegate void MediaPlayerTrackChangeHandler(int index);
    public delegate void MediaPlayerMediaOpenHandler();
    public delegate void StreamUpdatedHandler(NowPlayingSong song);

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

    public class PlaybackService
    {
        static PlaybackService instance;

        public static PlaybackService Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlaybackService();

                return instance;
            }
        }

        /// <summary>
        /// This application only requires a single shared MediaPlayer
        /// that all pages have access to. The instance could have 
        /// also been stored in Application.Resources or in an 
        /// application defined data model.
        /// </summary>
        public MediaPlayer Player { get; private set; }

        TimeSpan startPosition;
        private DateTime songStartedAt;
        private TimeSpan songPlayed;

        private bool paused;
        private bool isFirst;

        bool shuffle;
        RepeatEnum repeat;
        bool isPlaylistRepeated;
        bool isSongRepeated;

        private TimeSpan timePreviousOrBeggining = TimeSpan.FromSeconds(5);

        private AppState foregroundAppState = AppState.Unknown;
        public JamendoRadiosData jRadioData;
        private LastFmCache lastFmCache;

        private const int maxSongsNumber = 5;
        private const int playingSongIndex = 2;

        private bool? canPlay = null;

        private const string propertyIndex = "index";
        private const string propertySongId = "songid";
        
        /// <summary>
        /// The data model of the active playlist. An application might
        /// have a database of items representing a user's media library,
        /// but here we use a simple list loaded from a JSON asset.
        /// </summary>
        //public MediaList CurrentPlaylist { get; set; }

        MediaPlaybackList mediaList;
        private bool isGaplessPlaybackReady = false;
        private void CheckForGaplessPlaybackReady()
        {
            int i = 0;
            foreach (var song in NowPlayingPlaylistManager.Current.songs)
            {
                if (song.IsContentPathExpired())
                {
                    i++;
                }
                if (i == 5) break;
            }
            if (i == 5) isGaplessPlaybackReady = false;
            else isGaplessPlaybackReady = true;
        }

        public PlaybackService()
        {
            Logger.DebugWrite("PlaybackService","");
            
            // Create the player instance
            Player = new MediaPlayer();
            Player.AutoPlay = false;
            Player.MediaEnded += Player_MediaEnded;
            Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            Player.CommandManager.NextReceived += CommandManager_NextReceived;
            Player.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
            
            mediaList = new MediaPlaybackList();
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;

            shuffle = Shuffle.CurrentState();
            repeat = Repeat.CurrentState();
            isSongRepeated = false;

            jRadioData = new JamendoRadiosData();
            lastFmCache = new LastFmCache();

            songPlayed = TimeSpan.Zero;
            songStartedAt = DateTime.Now;
        }

        public async Task Initialize()
        {
            CheckForGaplessPlaybackReady();
            if (isGaplessPlaybackReady)
            {
                await LoadAll(CurrentSongIndex);
                Player.Source = mediaList;
            }
            else
            {
                var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                if (song.IsContentPathExpired())
                {
                    canPlay = false;
                    return;
                }
                else
                {
                    Player.Source = await PreparePlaybackItem(song);
                }
            }

            if (NowPlayingPlaylistManager.Current.songs.Count > 0)
            {
                Player.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
                Player.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            }

            canPlay = true;
        }

        Queue<MediaPlaybackItem> playbackItemQueue = new Queue<MediaPlaybackItem>();
        int maxCachedItems = 3;

        private void ManageQueue()
        {
            var item = mediaList.CurrentItem;
            if (item != null)
            {
                if (!playbackItemQueue.Contains(item))
                {
                    if (playbackItemQueue.Count == maxCachedItems)
                    {
                        var removed = playbackItemQueue.Dequeue();
                        removed.Source.Reset();
                    }
                    playbackItemQueue.Enqueue(item);
                }
            }
        }

        private void ClearQueue()
        {
            foreach(var song in playbackItemQueue)
            {
                song.Source.Reset();
            }
            playbackItemQueue.Clear();
        }

        public static bool IsTypeDefaultSupported(string type)
        {
            return (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                    type == ".wav" || type == ".aac" || type == ".asf" || type == ".flac" ||
                    type == ".adt" || type == ".adts" || type == ".amr" || type == ".mp4");
        }

        private async void MediaList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("MediaList_CurrentItemChanged {0} {1}", args.OldItem?.Source?.CustomProperties[propertyIndex] ?? "null", args.NewItem?.Source?.CustomProperties[propertyIndex] ?? "null");
            if (!command)//song finished
            {
                if (args.NewItem != null && args.OldItem != null)
                {
                    await Next(false);
                }
            }
            ManageQueue();
            command = false;
        }
        #region Events
        private async void Player_MediaEnded(MediaPlayer sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("Player_MediaEnded {0}", sender.PlaybackSession.PlaybackState);
            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            ScrobbleTrack();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;
            if (!isGaplessPlaybackReady)
            {
                await Next(false);
            }
        }

        private async void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            args.Handled = true;
            await Previous();
            deferral.Complete();
        }

        private async void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            args.Handled = true;
            await Next();
            deferral.Complete();
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            var state = sender.PlaybackState;
            System.Diagnostics.Debug.WriteLine("PlaybackSession_PlaybackStateChanged {0}", state);
            OnMediaPlayerStateChanged(state);
        }

        public static event MediaPlayerStateChangeHandler MediaPlayerStateChanged;
        public void OnMediaPlayerStateChanged(MediaPlaybackState state)
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerTrackChanged {0}", state);
            MediaPlayerStateChanged?.Invoke(state);
        }
        public static event MediaPlayerTrackChangeHandler MediaPlayerTrackChanged;
        public void OnMediaPlayerTrackChanged(int index)
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerTrackChanged {0}", index);
            MediaPlayerTrackChanged?.Invoke(index);
        }
        public static event MediaPlayerMediaOpenHandler MediaPlayerMediaOpened;
        public void OnMediaPlayerMediaOpened()
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerMediaOpened");
            MediaPlayerMediaOpened?.Invoke();
        }
        public static event StreamUpdatedHandler StreamUpdated;
        public void OnStreamUpdated(NowPlayingSong song)
        {
            System.Diagnostics.Debug.WriteLine("OnStreamUpdated");
            StreamUpdated?.Invoke(song);
        }
        #endregion

        private int CurrentSongIndex
        {
            get
            {
                return ApplicationSettingsHelper.ReadSongIndex();
            }
            set
            {
                ApplicationSettingsHelper.SaveSongIndex(value);
                OnMediaPlayerTrackChanged(value);
            }
        }

        public bool IsFirst
        {
            get { return CurrentSongIndex == 0; }
        }

        public bool IsLast
        {
            get { return CurrentSongIndex == NowPlayingPlaylistManager.Current.songs.Count - 1; }
        }

        public MediaPlaybackState PlayerState
        {
            get
            {
                return Player.PlaybackSession.PlaybackState;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return Player.PlaybackSession.NaturalDuration;
            }
        }

        public TimeSpan Position
        {
            get
            {
                return Player.PlaybackSession.Position;
            }
            set
            {
                Player.PlaybackSession.Position = value;
            }
        }

        public void Play()
        {
            if (canPlay == true)
            {
                songStartedAt = DateTime.Now;
                Player.Play();
            }
            else if(canPlay == false)
            {
                LoadAndPlay();
            }
        }

        private async Task LoadAndPlay()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            Player.Source = await PreparePlaybackItem(song);
            canPlay = true;
            songStartedAt = DateTime.Now;
            Player.Play();
            Player.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            Player.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
        }

        public void Pause()
        {
            Player.Pause();
            songPlayed = DateTime.Now - songStartedAt + songPlayed;
        }

        private bool command = false;
        public async Task Next(bool userChoice = true)
        {
            command = true;
            System.Diagnostics.Debug.WriteLine("Next");

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            ScrobbleTrack();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

            if (repeat == RepeatEnum.NoRepeat)
            {
                if (IsLast)
                {
                    if (userChoice)
                    {
                        if (isGaplessPlaybackReady)
                        {
                            mediaList.MoveNext();
                            CurrentSongIndex = 0;
                        }
                        else
                        {
                            CurrentSongIndex = 0;
                            bool isPlaying = Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                            Player.Source = null;
                            Player.Pause();
                            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                            Player.Source = await PreparePlaybackItem(song);
                            if (isPlaying) Player.Play();
                        }
                    }
                    else
                    {
                        if (mediaList.Items.Count > 1)
                        {
                            if (isGaplessPlaybackReady)
                            {
                                mediaList.MovePrevious();
                            }
                            else
                            {

                            }
                        }
                        
                        Player.Pause();
                        Player.PlaybackSession.Position = TimeSpan.Zero;
                    }
                }
                else
                {
                    if (userChoice)
                    {
                        if (isGaplessPlaybackReady)
                        {
                            mediaList.MoveNext();
                            CurrentSongIndex++;
                        }
                        else
                        {
                            CurrentSongIndex++;
                            bool isPlaying = Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                            Player.Source = null;
                            Player.Pause();
                            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                            Player.Source = await PreparePlaybackItem(song);
                            if (isPlaying) Player.Play();
                        }
                    }
                    else
                    {
                        CurrentSongIndex++;
                        if (!isGaplessPlaybackReady)
                        {
                            Player.Source = null;
                            Player.Pause();
                            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                            Player.Source = await PreparePlaybackItem(song);
                            Player.Play();
                        }
                    }
                }
            }
            else if (repeat == RepeatEnum.RepeatOnce)
            {
                if (IsLast)
                {
                    if (userChoice)
                    {
                        if (isGaplessPlaybackReady)
                        {
                            mediaList.MoveNext();
                            CurrentSongIndex = 0;
                        }
                        else
                        {
                            CurrentSongIndex = 0;
                            bool isPlaying = Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                            Player.Source = null;
                            Player.Pause();
                            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                            Player.Source = await PreparePlaybackItem(song);
                            if (isPlaying) Player.Play();
                        }
                    }
                    else
                    {
                        if (mediaList.Items.Count > 1)
                        {
                            if (isGaplessPlaybackReady)
                            {
                                mediaList.MovePrevious();
                            }
                            else
                            {

                            }
                        }
                        
                        Player.Pause();
                        Player.PlaybackSession.Position = TimeSpan.Zero;
                    }
                }
                else
                {
                    if (userChoice)
                    {
                        mediaList.MoveNext();
                        CurrentSongIndex++;
                    }
                    else
                    {
                        CurrentSongIndex++;
                        bool isPlaying = Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                        Player.Source = null;
                        Player.Pause();
                        var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                        Player.Source = await PreparePlaybackItem(song);
                        if (isPlaying) Player.Play();
                    }
                }
                //if (isSongRepeated)
                //{
                //    isSongRepeated = false;
                //    if (IsLast)
                //    {
                //        if (userChoice)
                //        {
                //            mediaList.MoveNext();
                //            await UpdateMediaList();
                //            CurrentSongIndex = 0;
                //        }
                //        else
                //        {
                //            mediaList.MovePrevious();
                //        }
                //    }
                //    else
                //    {
                //        if (userChoice)
                //        {
                //            mediaList.MoveNext();
                //        }
                //        await UpdateMediaList(false);
                //        CurrentSongIndex++;
                //    }
                //}
                //else
                //{
                //    if (userChoice)
                //    {
                //        mediaList.MoveNext();
                //        await UpdateMediaList();
                //        if (IsLast)
                //        {
                //            CurrentSongIndex = 0;
                //        }
                //        else
                //        {
                //            CurrentSongIndex++;
                //        }
                //    }
                //    else
                //    {
                //        mediaList.MovePrevious();
                //        isSongRepeated = true;
                //    }
                //}
            }
            else if (repeat == RepeatEnum.RepeatPlaylist)
            {
                if (IsLast)
                {
                    CurrentSongIndex = 0;
                }
                else
                {
                    CurrentSongIndex++;
                }
                if (userChoice)
                {
                    if (isGaplessPlaybackReady)
                    {
                        mediaList.MoveNext();

                    }
                    else
                    {
                        bool isPlaying = Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                        Player.Source = null;
                        Player.Pause();
                        var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                        Player.Source = await PreparePlaybackItem(song);
                        if (isPlaying) Player.Play();
                    }
                }
                else
                {
                    if (!isGaplessPlaybackReady)
                    {
                        Player.Source = null;
                        Player.Pause();
                        var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                        Player.Source = await PreparePlaybackItem(song);
                        Player.Play();
                    }
                }
            }
            
            OnMediaPlayerMediaOpened();
        }

        public async Task Previous()
        {
            System.Diagnostics.Debug.WriteLine("Previous");
            command = true;

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            ScrobbleTrack();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

            var session = Player.PlaybackSession;
            if (session.Position > timePreviousOrBeggining)
            {
                session.Position = TimeSpan.Zero;
            }
            else
            {
                isSongRepeated = false;
                
                if (isGaplessPlaybackReady)
                {
                    mediaList.MovePrevious();
                    if (CurrentSongIndex == 0)
                    {
                        CurrentSongIndex = NowPlayingPlaylistManager.Current.songs.Count - 1;
                    }
                    else
                    {
                        CurrentSongIndex--;
                    }
                }
                else
                {
                    if (CurrentSongIndex == 0)
                    {
                        CurrentSongIndex = NowPlayingPlaylistManager.Current.songs.Count - 1;
                    }
                    else
                    {
                        CurrentSongIndex--;
                    }
                    bool isPlaying = Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                    Player.Source = null;
                    Player.Pause();
                    var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                    Player.Source = await PreparePlaybackItem(song);
                    if (isPlaying) Player.Play();
                }
                //await UpdateMediaList(true);
                
                OnMediaPlayerMediaOpened();
            }
        }

        public async Task ChangeShuffle()
        {
            shuffle = !shuffle;
            System.Diagnostics.Debug.WriteLine("ChangeShuffle {0}", shuffle);
            if (!shuffle)
            {
                CurrentSongIndex = await NowPlayingPlaylistManager.Current.UnShufflePlaylist();
            }
            else
            {
                CurrentSongIndex = await NowPlayingPlaylistManager.Current.ShufflePlaylist();
            }
            await UpdateMediaListWithoutPausing();
        }

        public void ChangeRepeat()
        {
            repeat = Repeat.CurrentState();
            if (repeat == RepeatEnum.RepeatOnce)
            {
                Player.IsLoopingEnabled = true;
            }
            else
            {
                Player.IsLoopingEnabled = false;
            }
            System.Diagnostics.Debug.WriteLine("ChangeRepeat {0}", repeat);
            isSongRepeated = false;
        }

        public void ChangeVolume(int volume)
        {
            if (volume < 0 || volume > 100)
            {
                return;
            }
            Player.Volume = volume / 100.0;
        }

        public void TogglePlayPause()
        {
            switch (Player.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    Pause();
                    break;
                case MediaPlaybackState.Paused:
                    Play();
                    break;
                default:
                    try
                    {
                        Play();
                    }
                    catch { }
                    break;
            }
        }

        /// <summary>
        /// -1 balance 1
        /// </summary>
        /// <param name="balance"></param>
        public void AudioBalance(double balance)
        {
            Player.AudioBalance = balance;
        }

        public void ResetBalance()
        {
            Player.AudioBalance = 0;
        }


        ThreadPoolTimer playbackTimer = null;
        bool isPlaybackTimerSet = false;

        public void SetPlaybackTimer()
        {
            var t = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerTime);
            long timerTicks = 0;
            if (t != null)
            {
                timerTicks = (long)t;
            }

            TimeSpan currentTime = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute) + TimeSpan.FromSeconds(DateTime.Now.Second);

            TimeSpan delay = TimeSpan.FromTicks(timerTicks - currentTime.Ticks);
            if (delay < TimeSpan.Zero)
            {
                delay = delay + TimeSpan.FromHours(24);
            }
            if (delay > TimeSpan.Zero)
            {
                if (isPlaybackTimerSet)
                {
                    CancelPlaybackTimer();
                }
                playbackTimer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(PlaybackTimerCallback), delay);
                isPlaybackTimerSet = true;
            }
        }

        private void PlaybackTimerCallback(ThreadPoolTimer timer)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            CancelPlaybackTimer();
            Pause();
        }

        public void CancelPlaybackTimer()
        {
            isPlaybackTimerSet = false;
            if (playbackTimer != null)
            {
                playbackTimer.Cancel();
            }
        }

        public void UpdateTile()
        {
            //var displayProperties = mlist.CurrentItem.Source.CustomProperties[songid];
            //if (path != AppConstants.AlbumCover)
            //{
            //    myTileUpdater.UpdateAppTileBG(displayProperties.MusicProperties.Title, displayProperties.MusicProperties.Artist, AppConstants.AppLogoMedium);
            //}
            //else
            //{
            //    myTileUpdater.UpdateAppTileBG(displayProperties.MusicProperties.Title, displayProperties.MusicProperties.Artist, AppConstants.AppLogoMedium);
            //}
        }

        #region mediaList

        private void ClearMediaList()
        {
            ClearQueue();
            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            foreach (var item in mediaList.Items)
            {
                item.Source.OpenOperationCompleted -= Source_OpenOperationCompleted;
            }
            mediaList.Items.Clear();
        }

        private async Task LoadAll(int startIndex)
        {
            ClearMediaList();
            mediaList = new MediaPlaybackList();
            int ind = 0;
            foreach(var song in NowPlayingPlaylistManager.Current.songs)
            {
                var playbackItem = await PreparePlaybackItem(song);
                playbackItem.Source.CustomProperties[propertyIndex] = ind;
                mediaList.Items.Add(playbackItem);
                ind++;
            }
            mediaList.StartingItem = mediaList.Items.FirstOrDefault(i => i.Source.CustomProperties[propertyIndex].Equals(startIndex));
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
        }


        public async Task UpdateMediaListWithoutPausing()
        {
            if (!isGaplessPlaybackReady) return;

            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            uint j = mediaList.CurrentItemIndex;
            if (mediaList.CurrentItemIndex > mediaList.Items.Count)
            {
                j = (uint)mediaList.Items.IndexOf(mediaList.StartingItem);
            }
            for (int i = 0; i < mediaList.Items.Count; i++)
            {
                if (i != j)
                {
                    mediaList.Items[i].Source.Reset();
                }
            }

            while (j > 0)
            {
                mediaList.Items.RemoveAt(0);
                j--;
            }
            while (mediaList.Items.Count > 1)
            {
                mediaList.Items.RemoveAt(mediaList.Items.Count-1);
            }

            var currentIndex = CurrentSongIndex;
            var count = NowPlayingPlaylistManager.Current.songs.Count;
            for (int i = 0; i < count; i++)
            {
                if (i != currentIndex)
                {
                    var song = NowPlayingPlaylistManager.Current.songs[i];
                    var playbackItem = await PreparePlaybackItem(song);
                    playbackItem.Source.CustomProperties[propertyIndex] = i;
                    mediaList.Items.Insert(i, playbackItem);
                }
                else
                {
                    mediaList.Items[i].Source.CustomProperties[propertyIndex] = i;
                }
            }
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
        }

        public async  Task JumpTo(int startIndex)//nie zmienia stanu na playing!!
        {
            System.Diagnostics.Debug.WriteLine("JumpTo {0}", startIndex);
            command = true;

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            ScrobbleTrack();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

            if (isGaplessPlaybackReady)
            {
                mediaList.MoveTo((uint)startIndex);
            }
            else
            {
                Player.Pause();
                Player.Source = null;
                var song = NowPlayingPlaylistManager.Current.songs[startIndex];
                Player.Source = await PreparePlaybackItem(song);
            }
            
            CurrentSongIndex = startIndex;
            Player.Play();
            OnMediaPlayerMediaOpened();
        }

        public async Task PlayNewList(int startIndex, bool startPlaying = true)
        {
            System.Diagnostics.Debug.WriteLine("PlayNewList {0}", startIndex);
            command = true;
            shuffle = false;
            Player.Pause();

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            ScrobbleTrack();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

            Player.Source = null;
            CurrentSongIndex = startIndex;
            CheckForGaplessPlaybackReady();
            if (isGaplessPlaybackReady)
            {
                await LoadAll(startIndex);
                Player.Source = mediaList;
            }
            else
            {
                var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                Player.Source = await PreparePlaybackItem(song);
            }
            
            if (startPlaying)
            {
                Player.Play();
                OnMediaPlayerMediaOpened();
            }
        }

        private static async Task<MediaPlaybackItem> PreparePlaybackItem(SongItem song)
        {
            return await PlaybackItemBuilder.PreparePlaybackItem(song);
        }

        public static async void Source_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("UpdateMediaList {0}", sender.IsOpen);
            if (sender.IsOpen)
            {
                Instance.OnMediaPlayerMediaOpened();
            }
            else
            {
                //Instance.Player.Pause();
                //NextPlayerUWPDataLayer.CloudStorage.CloudStorageServiceFactory cssf = new NextPlayerUWPDataLayer.CloudStorage.CloudStorageServiceFactory();
                //var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                //var service = cssf.GetService(NextPlayerUWPDataLayer.CloudStorage.CloudStorageType.Dropbox, song.CloudUserId);
                //var link = await service.GetDownloadLink(song.Path);
                //var dropboxSource = MediaSource.CreateFromUri(new Uri(link));
                //var dropboxPlaybackItem = new MediaPlaybackItem(dropboxSource);
                //dropboxPlaybackItem.Source.OpenOperationCompleted += PlaybackService.Source_OpenOperationCompleted;
                ////UpdateDisplayProperties(dropboxPlaybackItem, song);
                //Instance.mediaList.CurrentItemChanged -= Instance.MediaList_CurrentItemChanged;
                //dropboxSource.CustomProperties[propertySongId] = song.SongId;
                //Instance.mediaList.Items.Insert((int)Instance.mediaList.CurrentItemIndex + 1, dropboxPlaybackItem);
                //Instance.mediaList.Items.RemoveAt((int)Instance.mediaList.CurrentItemIndex);
                //Instance.mediaList.CurrentItemChanged += Instance.MediaList_CurrentItemChanged;
                //Instance.Player.Play();

            }
        }

        public static void RadioSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("UpdateMediaList {0}", sender.IsOpen);
            if (sender.IsOpen)
            {
                Instance.SetTimer(500);
            }
        }

        #region Timer

        ThreadPoolTimer timer = null;
        bool isTimerSet = false;

        private void SetTimer(int ms)
        {
            ms += 100;
            TimeSpan currentTime = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute) + TimeSpan.FromSeconds(DateTime.Now.Second);

            if (isTimerSet)
            {
                TimerCancel();
            }
            timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(TimerCallback), TimeSpan.FromMilliseconds(ms));
            isTimerSet = true;
        }

        private async void TimerCallback(ThreadPoolTimer timer)
        {
            TimerCancel();
            await RefreshRadioTrackInfo();
        }

        private void TimerCancel()
        {
            isTimerSet = false;
            if (timer != null)
            {
                timer.Cancel();
            }
        }

        #endregion

        private async Task RefreshRadioTrackInfo()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (song.SourceType == MusicSource.RadioJamendo)
            {
                var stream = await jRadioData.GetRadioStream(song.SongId);
                if (stream == null) return;
                var radio = jRadioData.GetRadioItemFromStream(stream);

                var displ = mediaList.CurrentItem.GetDisplayProperties();
                displ.MusicProperties.AlbumTitle = stream.Album;
                displ.MusicProperties.Artist = stream.Artist;
                try
                {
                    displ.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(stream.CoverUri));
                }
                catch
                {
                    displ.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
                }
                mediaList.CurrentItem.ApplyDisplayProperties(displ);

                NowPlayingSong s = new NowPlayingSong()
                {
                    Album = stream.Album,
                    Artist = stream.Artist + " - " + stream.Title,
                    ImagePath = stream.CoverUri,
                    Path = song.Path,
                    Position = CurrentSongIndex,
                    SongId = song.SongId,
                    SourceType = MusicSource.RadioJamendo,
                    Title = song.Title,
                };

                OnStreamUpdated(s);

                int ms = jRadioData.GetRemainingSeconds(stream);
                SetTimer(ms);
            }
        }

        private void ScrobbleTrack()
        {
            System.Diagnostics.Debug.WriteLine("ScrobbleTrack started at:{0} played:{1}", songStartedAt, songPlayed);


            //if (CanFlagSongAsPlayed(songPlayed) && (song.SourceType == MusicSource.LocalFile || song.SourceType == MusicSource.LocalNotMusicLibrary))
            //{
            //    await UpdateSongStatistics(song.SongId, songDuration);
            //    if (mediaList.CurrentItem.Source.Duration > TimeSpan.FromSeconds(30) && lastFmCache.AreCredentialsSet())
            //    {
            //        await CacheTrackScrobble(song);
            //    }
            //}
        }

        private async Task CacheTrackScrobble(int index)
        {
            var song = NowPlayingPlaylistManager.Current.songs[index];
            int seconds = 0;
            try
            {
                DateTime start = DateTime.UtcNow - songPlayed;
                seconds = (int)start.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            }
            catch (Exception ex)
            {
                //Diagnostics.Logger.SaveBG("Scrobble paused " + songPlayed + Environment.NewLine + ex.Data + Environment.NewLine + ex.Message);
                //Diagnostics.Logger.SaveToFileBG();
                return;
            }
            string artist = song.Artist;
            string track = song.Title;
            string timestamp = seconds.ToString();
            TrackScrobble scrobble = new TrackScrobble()
            {
                Artist = artist,
                Track = track,
                Timestamp = timestamp
            };
            await lastFmCache.CacheTrackScrobble(scrobble);
            System.Diagnostics.Debug.WriteLine("scrobble " + artist + " " + track + " " + songPlayed);
        }

        private bool CanFlagSongAsPlayed(TimeSpan totalTimePlayed)
        {
            return (songPlayed.TotalSeconds >= totalTimePlayed.TotalSeconds * 0.5 || songPlayed.TotalSeconds >= 4 * 60);
        }

        #endregion
    }
}
