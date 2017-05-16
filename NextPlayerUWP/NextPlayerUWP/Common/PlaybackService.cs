using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace NextPlayerUWP.Common
{
    public delegate void MediaPlayerStateChangeHandler(MediaPlaybackState state);
    public delegate void MediaPlayerTrackChangeHandler(int index);
    public delegate void MediaPlayerMediaOpenHandler();
    public delegate void StreamUpdatedHandler(NowPlayingSong song);

    public partial class PlaybackService
    {
        private static PlaybackService instance;
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
        private MediaPlaybackList mediaList;

        private class InfoForTask
        {
            public int LoadedFromIndex { get; set; }
            public int LoadedToIndex { get; set; }
            public int MaxIndex { get; set; }
        }
        private InfoForTask infoForTask;
        private CancellationTokenSource cts;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private const string propertyIndex = "index";
        private const string propertySongId = "songid";

        public PlaybackService()
        {
            Logger2.DebugWrite("PlaybackService()","");
            
            // Create the player instance
            Player = new MediaPlayer();
            Player.AutoPlay = false;
            Player.MediaEnded += Player_MediaEnded;
            Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            Player.CommandManager.NextReceived += CommandManager_NextReceived;
            Player.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
            Player.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            Player.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;

            mediaList = new MediaPlaybackList();
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;

            shuffle = Shuffle.CurrentState();
            ApplyRepeatState();

            lastFmCache = new LastFmCache();

            songPlayingStoper = new Stoper();

            RadioTimer = new PlaybackTimer();
            MusicPlaybackTimer = new PlaybackTimer();

            cts = new CancellationTokenSource();
        }

        public async Task Initialize()
        {
            System.Diagnostics.Debug.WriteLine("PlaybackService.Initialize() Start");
            try
            {
                await semaphore.WaitAsync();
                try
                {
                    await LoadAll(CurrentSongIndex, cts.Token);
                    CancellationToken token = cts.Token;
                    await Task.Run(async () =>
                    {
                        var ctoken = token;
                        await LoadRest(infoForTask, ctoken);
                    }, token);
                    Player.Source = mediaList;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Initialize Cancelled");
            }
            System.Diagnostics.Debug.WriteLine("PlaybackService.Initialize() End");
        }

        private const int maxCachedItems = 3;
        private Queue<MediaPlaybackItem> playbackItemQueue = new Queue<MediaPlaybackItem>();

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

        #region Events

        private void MediaList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("MediaList_CurrentItemChanged {0} {1}", args.OldItem?.Source?.CustomProperties[propertyIndex] ?? "null", args.NewItem?.Source?.CustomProperties[propertyIndex] ?? "null");
            if (repeat == RepeatEnum.RepeatPlaylist &&
                args.NewItem != null && args.OldItem != null && 
                (int)args.NewItem.Source.CustomProperties[propertyIndex] == 0 && 
                (int)args.OldItem.Source.CustomProperties[propertyIndex] == sender.Items.Count)
            {
                CurrentSongIndex = 0;
            }
            if (args.NewItem != null)
            {
                CurrentSongIndex = (int)args.NewItem.Source.CustomProperties[propertyIndex];
                if (startPlay)
                {
                    startPlay = false;
                    Play();
                }
            }
            songPlayingStoper.Stop();
            if (args.OldItem != null)
            {
                int id = (int)args.OldItem.Source.CustomProperties[propertySongId];
                TimeSpan duration = args.OldItem.Source?.Duration ?? TimeSpan.Zero;
                UpdateStats(id, duration, songPlayingStoper.ElapsedTime);
            }
            songPlayingStoper.ResetAndStart();
            OnMediaPlayerMediaOpened();
            ManageQueue();
        }
        
        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("Player_MediaEnded {0}", sender.PlaybackSession.PlaybackState);
            songPlayingStoper.Stop();
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            UpdateStats(song.SongId, Duration, songPlayingStoper.ElapsedTime);
            songPlayingStoper.ResetAndStart();
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
            OnMediaPlayerStateChanged(sender.PlaybackState);
        }

        public static event MediaPlayerStateChangeHandler MediaPlayerStateChanged;
        public void OnMediaPlayerStateChanged(MediaPlaybackState state)
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerStateChanged {0}", state);
            MediaPlayerStateChanged?.Invoke(state);
        }

        public static event MediaPlayerMediaOpenHandler MediaPlayerMediaOpened;
        public void OnMediaPlayerMediaOpened()
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerMediaOpened {0} {1}", Player.PlaybackSession.PlaybackState, Player.PlaybackSession.NaturalDuration);
            MediaPlayerMediaOpened?.Invoke();
        }
        public static event StreamUpdatedHandler StreamUpdated;
        public void OnStreamUpdated(NowPlayingSong song)
        {
            System.Diagnostics.Debug.WriteLine("OnStreamUpdated");
            StreamUpdated?.Invoke(song);
        }
        #endregion
       
        #region mediaList

        bool isLoaded = false; //TODO remove?

        //doesn't change state to Playing
        public async Task JumpTo(int startIndex)
        {
            System.Diagnostics.Debug.WriteLine("JumpTo {0}", startIndex);

            if (!isLoaded)
            {
                if (startIndex > mediaList.Items.Count)
                {
                    return;
                }
                for (int i = 0; i < 50; i++)
                {
                    if ((int)mediaList.Items[i].Source.CustomProperties[propertyIndex] != i) return; //?
                }
            }

            if (Player.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                //mediaList.StartingItem = mediaList.Items[startIndex];
                mediaList.MoveTo((uint)startIndex);
                startPlay = true;
                //Play();
            }
            else
            {
                mediaList.MoveTo((uint)startIndex);
            }

            CurrentSongIndex = startIndex;
            OnMediaPlayerMediaOpened();
        }


        public async Task PlayNewList(int startIndex, bool startPlaying = true)
        {
            System.Diagnostics.Debug.WriteLine("PlayNewList {0}", startIndex);
            Stopwatch s = new Stopwatch();
            s.Start();
            cts.Cancel();
            cts = new CancellationTokenSource();

            Player.Pause();

            songPlayingStoper.Stop();
            int id = (int)(mediaList.CurrentItem?.Source?.CustomProperties[propertySongId] ?? -1);
            UpdateStats(id, Duration, songPlayingStoper.ElapsedTime);
            songPlayingStoper.ResetAndStart();

            shuffle = Shuffle.CurrentState();
            if (shuffle)
            {
                startIndex = await NowPlayingPlaylistManager.Current.ShufflePlaylist(startIndex);
            }
            CurrentSongIndex = startIndex;
            await semaphore.WaitAsync();
            try
            {
                Player.Source = null;
                CancellationToken token = cts.Token;
                try
                {
                    await LoadAll(startIndex, cts.Token);
                    Player.Source = mediaList;

                    if (startPlaying)
                    {
                        Player.Play();
                        OnMediaPlayerMediaOpened();
                    }

                    await Task.Run(async () =>
                    {
                        var ctoken = token;
                        await LoadRest(infoForTask, ctoken);
                    }, token);
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("PlayNewList Cancelled");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("PlayNewList ERROR");
                }
            }
            finally
            {
                semaphore.Release();
            }
            s.Stop();
            Debug.WriteLine("PlayNewList End {0}ms", s.ElapsedMilliseconds);
        }

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

        private async Task LoadAll(int startIndex, CancellationToken token)
        {
            Stopwatch s1 = new Stopwatch();
            s1.Start();
            ClearMediaList();
            infoForTask = await LoadMiddle(startIndex, token).ConfigureAwait(false);
            mediaList.StartingItem = mediaList.Items.FirstOrDefault(i => i.Source.CustomProperties[propertyIndex].Equals(startIndex));
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
            s1.Stop();
            Debug.WriteLine("LoadAll {0}", s1.ElapsedMilliseconds);
        }

        private const int countOfPreloadedSongs = 40;
        private const int halfOfCount = 20; //countOfPreLoadedSongsBeforeCurrentPlaying

        private async Task<InfoForTask> LoadMiddle(int startIndex, CancellationToken token)
        {
            isLoaded = false;
            int maxIndex = NowPlayingPlaylistManager.Current.songs.Count - 1;
            int currentIndex = CurrentSongIndex;
            InfoForTask info = null;
            if (maxIndex <= countOfPreloadedSongs)
            {
                await AppendToMediaList(0, maxIndex, token);
                isLoaded = true;
            }
            else
            {
                if (currentIndex < halfOfCount)
                {
                    await AppendToMediaList(0, countOfPreloadedSongs, token);
                    info = new InfoForTask()
                    {
                        LoadedFromIndex = 0,
                        LoadedToIndex = countOfPreloadedSongs,
                        MaxIndex = maxIndex,
                    };
                }
                else
                {
                    int endBasicLoad = (currentIndex + halfOfCount < maxIndex) ? currentIndex + halfOfCount : maxIndex;
                    await AppendToMediaList(currentIndex - halfOfCount, endBasicLoad, token);
                    info = new InfoForTask()
                    {
                        LoadedFromIndex = currentIndex - halfOfCount,
                        LoadedToIndex = endBasicLoad,
                        MaxIndex = maxIndex,
                    };
                }
            }
            return info;
        }

        private async Task LoadRest(InfoForTask info, CancellationToken token)
        {
            System.Diagnostics.Debug.WriteLine("LoadRest Start");
            Stopwatch s = new Stopwatch();
            s.Start();
            if (info == null) return;
            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            if (info.LoadedFromIndex == 0)
            {
                await AppendToMediaList(info.LoadedToIndex + 1, info.MaxIndex, token);
            }
            else
            {
                await InsertToMediaList(0, info.LoadedFromIndex - 1, token);
                if (info.LoadedToIndex < info.MaxIndex)
                {
                    await AppendToMediaList(info.LoadedToIndex + 1, info.MaxIndex, token);
                }
            }
            token.ThrowIfCancellationRequested();
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
            isLoaded = true;
            s.Stop();
            System.Diagnostics.Debug.WriteLine("LoadRest End {0}ms", s.ElapsedMilliseconds);
        }

        private async Task AppendToMediaList(int startIndex, int endIndex, CancellationToken token)
        {
            Debug.WriteLine("AddToMediaList from {0} to {1}", startIndex, endIndex);
            for (int i = startIndex; i <= endIndex; i++)
            {
                var song = NowPlayingPlaylistManager.Current.songs[i];
                var playbackItem = await PlaybackItemBuilder.PreparePlaybackItem(song);
                playbackItem.Source.CustomProperties[propertyIndex] = i;
                token.ThrowIfCancellationRequested();
                mediaList.Items.Add(playbackItem);
            }
        }

        private async Task InsertToMediaList(int startIndex, int endIndex, CancellationToken token)
        {
            Debug.WriteLine("InsertToMediaList from {0} to {1}", startIndex, endIndex);
            for (int i = startIndex; i <= endIndex; i++)
            {
                var song = NowPlayingPlaylistManager.Current.songs[i];
                var playbackItem = await PlaybackItemBuilder.PreparePlaybackItem(song);
                playbackItem.Source.CustomProperties[propertyIndex] = i;
                token.ThrowIfCancellationRequested();
                mediaList.Items.Insert(i, playbackItem);
            }
        }

        public async Task UpdateMediaListWithoutPausing()
        {
            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            uint j = mediaList.CurrentItemIndex;
            if (mediaList.CurrentItemIndex > mediaList.Items.Count)
            {
                j = (uint)mediaList.Items.IndexOf(mediaList.StartingItem);
            }
            if (j == UInt32.MaxValue)
            {
                j = 0;
            }
            if (j!= CurrentSongIndex)//??
            {

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
                    if (mediaList.Items.Count == 0)
                    {
                        var song = NowPlayingPlaylistManager.Current.songs[i];
                        var playbackItem = await PreparePlaybackItem(song);
                        playbackItem.Source.CustomProperties[propertyIndex] = i;
                        mediaList.Items.Add(playbackItem);
                    }
                    else
                    {
                        mediaList.Items[i].Source.CustomProperties[propertyIndex] = i;
                    }
                }
            }
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
        }

        private bool startPlay = false;

        #endregion
        private void RefreshRadioTrackInfo()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
        }
    }
}
