using FFmpegInterop;
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
using Windows.Media.Audio;
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
        private DateTime songsStart;
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

        private bool canPlay = false;

        private const string propertyIndex = "index";
        private const string propertySongId = "songid";
        
        /// <summary>
        /// The data model of the active playlist. An application might
        /// have a database of items representing a user's media library,
        /// but here we use a simple list loaded from a JSON asset.
        /// </summary>
        //public MediaList CurrentPlaylist { get; set; }

        MediaPlaybackList mediaList;

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
        }

        public async Task Initialize()
        {
            await LoadAll(CurrentSongIndex);
            //await LoadNewPlaylistItems(CurrentSongIndex);
            Player.Source = mediaList;
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

        private static bool IsTypeDefaultSupported(string type)
        {
            return (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                    type == ".wav" || type == ".aac" || type == ".asf" || type == ".flac" ||
                    type == ".adt" || type == ".adts" || type == ".amr" || type == ".mp4");
        }

        private void MediaList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("MediaList_CurrentItemChanged {0} {1}", args.OldItem?.Source?.CustomProperties[propertyIndex] ?? "null", args.NewItem?.Source?.CustomProperties[propertyIndex] ?? "null");
            if (!command)//song finished
            {
                if (args.NewItem != null && args.OldItem != null)
                {
                    Next(false);
                }
            }
            ManageQueue();
            command = false;
        }
        #region Events
        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("Player_MediaEnded"); 
        }

        private void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            //var deferral = args.GetDeferral();
            args.Handled = true;
            Previous();
            //deferral.Complete();
        }

        private void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            //var deferral = args.GetDeferral();
            args.Handled = true;
            Next();
            //deferral.Complete();
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
            if (canPlay)
            {
                Player.Play();
            }
        }

        public void Pause()
        {
            Player.Pause();
        }

        private bool command = false;
        public void Next(bool userChoice = true)
        {
            command = true;
            System.Diagnostics.Debug.WriteLine("Next");
            
            if (repeat == RepeatEnum.NoRepeat)
            {
                if (IsLast)
                {
                    if (userChoice)
                    {
                        mediaList.MoveNext();
                        //await UpdateMediaList();
                        CurrentSongIndex = 0;
                    }
                    else
                    {
                        if (mediaList.Items.Count > 1)
                        {
                            mediaList.MovePrevious();
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
                    }
                    //await UpdateMediaList(false);
                    CurrentSongIndex++;
                }
            }
            else if (repeat == RepeatEnum.RepeatOnce)
            {
                if (IsLast)
                {
                    if (userChoice)
                    {
                        mediaList.MoveNext();
                        //await UpdateMediaList();
                        CurrentSongIndex = 0;
                    }
                    else
                    {
                        if (mediaList.Items.Count > 1)
                        {
                            mediaList.MovePrevious();
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
                    }
                    //await UpdateMediaList(false);
                    CurrentSongIndex++;
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
                if (userChoice)
                {
                    mediaList.MoveNext();
                }
                //await UpdateMediaList();
                if (IsLast)
                {
                    CurrentSongIndex = 0;
                }
                else
                {
                    CurrentSongIndex++;
                }
            }
            
            OnMediaPlayerMediaOpened();
        }

        public void Previous()
        {
            System.Diagnostics.Debug.WriteLine("Previous");
            command = true;
            var session = Player.PlaybackSession;
            if (session.Position > timePreviousOrBeggining)
            {
                session.Position = TimeSpan.Zero;
            }
            else
            {
                isSongRepeated = false;
                
                mediaList.MovePrevious();
                //await UpdateMediaList(true);
                if (CurrentSongIndex == 0)
                {
                    CurrentSongIndex = NowPlayingPlaylistManager.Current.songs.Count - 1;
                }
                else
                {
                    CurrentSongIndex--;
                }
                OnMediaPlayerMediaOpened();
            }
        }

        public async Task ChangeShuffle()
        {
            shuffle = !shuffle;
            System.Diagnostics.Debug.WriteLine("ChangeShuffle {0}", shuffle);
            if (!shuffle)
            {
                await NowPlayingPlaylistManager.Current.UnShufflePlaylist();
            }
            else
            {
                await NowPlayingPlaylistManager.Current.ShufflePlaylist();
            }
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

        public void SetTimer()
        {

        }

        public void CancelTimer()
        {

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
                //item.Source.Reset();
                item.Source.OpenOperationCompleted -= Source_OpenOperationCompleted;
            }
            mediaList.Items.Clear();
        }

        private async Task LoadNewPlaylistItems(int startIndex)
        {
            ClearMediaList();
            mediaList = new MediaPlaybackList();
            
            if (NowPlayingPlaylistManager.Current.songs.Count <= maxSongsNumber)
            {
                for (int i = 0; i < NowPlayingPlaylistManager.Current.songs.Count; i++)
                {
                    var song = NowPlayingPlaylistManager.Current.songs[i];
                    var playbackItem = await PreparePlaybackItem(song);
                    playbackItem.Source.CustomProperties[propertyIndex] = i;
                    mediaList.Items.Add(playbackItem);
                }
            }
            else
            {
                int index = 0;
                int diff = maxSongsNumber / 2;
                for (int i = startIndex - diff; i < startIndex - diff + maxSongsNumber; i++)
                {
                    if (i < 0)
                    {
                        index = i + NowPlayingPlaylistManager.Current.songs.Count;
                    }
                    else if (i >= NowPlayingPlaylistManager.Current.songs.Count)
                    {
                        index = i - NowPlayingPlaylistManager.Current.songs.Count;
                    }
                    else
                    {
                        index = i;
                    }

                    var song = NowPlayingPlaylistManager.Current.songs[index];
                    var playbackItem = await PreparePlaybackItem(song);
                    playbackItem.Source.CustomProperties[propertyIndex] = index;
                    mediaList.Items.Add(playbackItem);
                }
            }
            mediaList.StartingItem = mediaList.Items.FirstOrDefault(i => i.Source.CustomProperties[propertyIndex].Equals(startIndex));
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;

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

        public async Task UpdateMediaListWithoutPausing2()
        {
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
                mediaList.Items.RemoveAt(1);
            }

            var currentIndex = CurrentSongIndex;
            var count = NowPlayingPlaylistManager.Current.songs.Count;
            if (count <= maxSongsNumber)
            {
                for(int i = 0; i < count; i++)
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
            }
            else
            {
                int nowPlayingPlaylistIndex = 0;
                int diff = maxSongsNumber / 2;
                j = 0;
                for (int i = currentIndex - diff; i < currentIndex - diff + maxSongsNumber; i++)
                {
                    if (j != 2)
                    {
                        if (i < 0)
                        {
                            nowPlayingPlaylistIndex = i + count;
                        }
                        else if (i >= NowPlayingPlaylistManager.Current.songs.Count)
                        {
                            nowPlayingPlaylistIndex = i - count;
                        }
                        else
                        {
                            nowPlayingPlaylistIndex = i;
                        }
                        var song = NowPlayingPlaylistManager.Current.songs[nowPlayingPlaylistIndex];
                        var playbackItem = await PreparePlaybackItem(song);
                        playbackItem.Source.CustomProperties[propertyIndex] = nowPlayingPlaylistIndex;
                        mediaList.Items.Insert((int)j, playbackItem);
                    }
                    j++;
                }
            }
            //bool isok = true;
            //for(int i= 0;i< mediaList.Items.Count;i++)
            //{
            //    //System.Diagnostics.Debug.WriteLine("3 Item {0} {1}", mediaList.Items[i].Source.CustomProperties[propertyIndex], mediaList.Items[i].Source.CustomProperties[propertySongId]);
            //    //System.Diagnostics.Debug.WriteLine("1 Item {0} {1}", i, NowPlayingPlaylistManager.Current.songs[i].SongId);
            //    int k = (int)mediaList.Items[i].Source.CustomProperties[propertyIndex];
            //    if (NowPlayingPlaylistManager.Current.songs[k].SongId != (int)mediaList.Items[i].Source.CustomProperties[propertySongId])
            //    {
            //        isok = false;
            //    }
            //}
            //if (!isok) System.Diagnostics.Debug.WriteLine("!!!!!!!!!");
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
        }

        public void JumpTo(int startIndex)
        {
            System.Diagnostics.Debug.WriteLine("JumpTo {0}", startIndex);
            command = true;
            mediaList.MoveTo((uint)startIndex);
            //Player.Pause();
            //Player.Source = null;
            CurrentSongIndex = startIndex;
            //await LoadNewPlaylistItems(startIndex);
            //Player.Source = mediaList;
            Player.Play();
            OnMediaPlayerMediaOpened();
        }

        public async Task PlayNewList(int startIndex, bool startPlaying = true)
        {
            System.Diagnostics.Debug.WriteLine("PlayNewList {0}", startIndex);
            command = true;
            shuffle = false;
            Player.Pause();
            Player.Source = null;
            CurrentSongIndex = startIndex;
            await LoadAll(startIndex);
            Player.Source = mediaList;
            if (startPlaying)
            {
                Player.Play();
                OnMediaPlayerMediaOpened();
            }
        }

        private async Task UpdateMediaList(bool prev = false)
        {
            System.Diagnostics.Debug.WriteLine("UpdateMediaList");
            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            if (mediaList.Items.Count == maxSongsNumber)
            {
                if (prev)
                {
                    mediaList.Items[maxSongsNumber - 1].Source.Reset();
                    mediaList.Items.RemoveAt(maxSongsNumber - 1);

                    int index = (int)mediaList.Items[0].Source.CustomProperties[propertyIndex];
                    if (index == 0)
                    {
                        index = NowPlayingPlaylistManager.Current.songs.Count - 1;
                    }
                    else
                    {
                        index--;
                    }
                    var song = NowPlayingPlaylistManager.Current.songs[index];
                    var playbackItem = await PreparePlaybackItem(song);
                    playbackItem.Source.CustomProperties[propertyIndex] = index;

                    mediaList.Items.Insert(0, playbackItem);
                    //foreach (var item in mediaList.Items)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Item index{0}", item.Source.CustomProperties[propertyIndex]);
                    //}

                }
                else
                {
                    mediaList.Items[0].Source.Reset();
                    mediaList.Items.RemoveAt(0);
                    int index = (int)mediaList.Items[mediaList.Items.Count - 1].Source.CustomProperties[propertyIndex];
                    if (index == NowPlayingPlaylistManager.Current.songs.Count - 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }
                    var song = NowPlayingPlaylistManager.Current.songs[index];
                    var playbackItem = await PreparePlaybackItem(song);
                    playbackItem.Source.CustomProperties[propertyIndex] = index;
                    mediaList.Items.Add(playbackItem);
                    //foreach (var item in mediaList.Items)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Item index{0}", item.Source.CustomProperties[propertyIndex]);
                    //}
                }
            }
            else
            {

            }
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
        }

        private static async Task<MediaPlaybackItem> PreparePlaybackItem(SongItem song)
        {
            return await PlaybackItemBuilder.PreparePlaybackItem(song);
        }

        public static void Source_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("UpdateMediaList {0}", sender.IsOpen);
            if (sender.IsOpen)
            {
                Instance.OnMediaPlayerMediaOpened();
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

        #endregion
    }
}
