using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Jamendo;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace NextPlayerUWP.Common
{
    public delegate void MediaPlayerStateChangeHandler(MediaPlaybackState state);
    public delegate void MediaPlayerTrackChangeHandler(int index);
    public delegate void MediaPlayerMediaOpenHandler();
    public delegate void StreamUpdatedHandler(NowPlayingSong song);
    
    public class InfoForTask
    {
        public int c { get; set; }
        public int max { get; set; }
        public int e { get; set; }
        public CancellationToken token { get; set; }
    }

    public partial class PlaybackService
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
        private bool paused;
        private bool isFirst;


        private const int maxSongsNumber = 5;
        private const int playingSongIndex = 2;
        private const string propertyIndex = "index";
        private const string propertySongId = "songid";

        private bool canPlay = false;

       
        
        MediaPlaybackList mediaList;

        bool isGaplessPlaybackReady = true;


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
            Player.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            Player.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;

            mediaList = new MediaPlaybackList();
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;

            shuffle = Shuffle.CurrentState();
            ApplyRepeatState();

            jRadioData = new JamendoRadiosData();
            lastFmCache = new LastFmCache();

            songPlayed = TimeSpan.Zero;
            songStartedAt = DateTime.Now;

            RadioTimer = new PlaybackTimer();
            MusicPlaybackTimer = new PlaybackTimer();

            cts = new CancellationTokenSource();
        }

        public async Task Initialize()
        {
            System.Diagnostics.Debug.WriteLine("PlaybackService Initialize Start");
            await LoadAll(CurrentSongIndex);
            await LoadRest(info);
            //System.Diagnostics.Debug.WriteLine("PlaybackService Initialize Loaded");
            Player.Source = mediaList;
            canPlay = true;
            System.Diagnostics.Debug.WriteLine("PlaybackService Initialize End");
        }

        int maxCachedItems = 3;
        Queue<MediaPlaybackItem> playbackItemQueue = new Queue<MediaPlaybackItem>();

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
            OnMediaPlayerMediaOpened();
            ManageQueue();
            command = false;
        }
        
        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("Player_MediaEnded {0}", sender.PlaybackSession.PlaybackState);
            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            UpdateStats();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;
        }
        #region Events
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

        public async Task ChangeShuffle()
        {
            shuffle = Shuffle.CurrentState();
            System.Diagnostics.Debug.WriteLine("ChangeShuffle {0}", shuffle);
            if (!shuffle)
            {
                //await NowPlayingPlaylistManager.Current.UnShufflePlaylist();
                //mediaList.ShuffleEnabled = false;
                CurrentSongIndex = await NowPlayingPlaylistManager.Current.UnShufflePlaylist();
            }
            else
            {
                CurrentSongIndex = await NowPlayingPlaylistManager.Current.ShufflePlaylist();
                //mediaList.ShuffleEnabled = true;
            }
            await UpdateMediaListWithoutPausing();
        }

        public void ApplyRepeatState()
        {
            repeat = Repeat.CurrentState();
            if (repeat == RepeatEnum.RepeatOnce)
            {
                Player.IsLoopingEnabled = true;
                mediaList.AutoRepeatEnabled = false;
            }
            else if (repeat == RepeatEnum.RepeatPlaylist)
            {
                Player.IsLoopingEnabled = false;
                mediaList.AutoRepeatEnabled = true;
            }
            else
            {
                Player.IsLoopingEnabled = false;
                mediaList.AutoRepeatEnabled = false;
            }
            System.Diagnostics.Debug.WriteLine("ChangeRepeat {0}", repeat);
        }

        private bool command = false;
        public void Next()
        {
            command = true;
            System.Diagnostics.Debug.WriteLine("Next");

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            UpdateStats();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

            mediaList.MoveNext();

            if (IsLastIndex)
            {
                CurrentSongIndex = 0;
            }
            else
            {
                CurrentSongIndex++;
            }

            //if (repeat == RepeatEnum.NoRepeat)
            //{
            //    if (IsLast)
            //    {
            //        if (userChoice)
            //        {
            //            mediaList.MoveNext();
            //            CurrentSongIndex = 0;
            //        }
            //        else
            //        {
            //            Player.Pause();
            //            Player.PlaybackSession.Position = TimeSpan.Zero;
            //        }
            //    }
            //    else
            //    {
            //        if (userChoice)
            //        {
            //            mediaList.MoveNext();
            //        }
            //        CurrentSongIndex++;
            //    }
            //}
            //else if (repeat == RepeatEnum.RepeatOnce)
            //{
            //    if (IsLast)
            //    {
            //        if (userChoice)
            //        {
            //            mediaList.MoveNext();
            //            CurrentSongIndex = 0;
            //        }
            //        else
            //        {
            //            Player.Pause();
            //            Player.PlaybackSession.Position = TimeSpan.Zero;
            //        }
            //    }
            //    else
            //    {
            //        if (userChoice)
            //        {
            //            mediaList.MoveNext();
            //        }
            //        CurrentSongIndex++;
            //    }
            //}
            //else if (repeat == RepeatEnum.RepeatPlaylist)
            //{
            //    if (userChoice)
            //    {
            //        mediaList.MoveNext();
            //    }
            //    if (IsLast)
            //    {
            //        CurrentSongIndex = 0;
            //    }
            //    else
            //    {
            //        CurrentSongIndex++;
            //    }
            //}

            OnMediaPlayerMediaOpened();
        }

        public void Previous()
        {
            System.Diagnostics.Debug.WriteLine("Previous");
            command = true;

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            UpdateStats();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

            var session = Player.PlaybackSession;
            if (session.Position > timePreviousOrBeggining)
            {
                session.Position = TimeSpan.Zero;
            }
            else
            {
                mediaList.MovePrevious();
                //if (CurrentSongIndex == 0)
                //{
                //    CurrentSongIndex = NowPlayingPlaylistManager.Current.songs.Count - 1;
                //}
                //else
                //{
                //    CurrentSongIndex--;
                //}
                OnMediaPlayerMediaOpened();
            }
        }

        #region mediaList

        CancellationTokenSource cts;
        bool isLoaded = false;

        public async Task JumpTo(int startIndex)//nie zmienia stanu na playing!!
        {
            System.Diagnostics.Debug.WriteLine("JumpTo {0}", startIndex);
            if (!canPlay) return;
            command = true;

            if (!isLoaded)
            {

                if (startIndex > mediaList.Items.Count)
                {
                    return;
                }
                for (int i = 0; i < 50; i++)
                {
                    if ((int)mediaList.Items[i].Source.CustomProperties[propertyIndex] != i) return;
                }
            }

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            UpdateStats();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;
            int q = mediaList.Items.Count;
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
            //Player.Source = await PreparePlaybackItem(NowPlayingPlaylistManager.Current.songs.FirstOrDefault());
            //return;
            command = true;
            cts.Cancel();
            Player.Pause();

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            UpdateStats();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

            Player.Source = null;
            CurrentSongIndex = startIndex;

            shuffle = Shuffle.CurrentState();
            if (shuffle)
            {
                startIndex = await NowPlayingPlaylistManager.Current.ShufflePlaylist();
            }

            await LoadAll(startIndex);
            Player.Source = mediaList;

            if (startPlaying)
            {
                Player.Play();
                OnMediaPlayerMediaOpened();
            }

            await LoadRest(info).ConfigureAwait(false);
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

        private async Task LoadAll(int startIndex)
        {
            cts = new CancellationTokenSource();
            ClearMediaList();
            //int index = 0;
            //foreach (var song in NowPlayingPlaylistManager.Current.songs)
            //{
            //    //s1.Start();
            //    var playbackItem = await PlaybackItemBuilder.PreparePlaybackItem(song);
            //    //s1.Stop();
            //    playbackItem.Source.CustomProperties[propertyIndex] = index;
            //    mediaList.Items.Add(playbackItem);
            //    index++;
            //}
            Stopwatch s1 = new Stopwatch();
            s1.Start();
            await LoadAll2(startIndex);
            s1.Stop();
            Debug.WriteLine("LoadAll {0}", s1.ElapsedMilliseconds);
            mediaList.StartingItem = mediaList.Items.FirstOrDefault(i => i.Source.CustomProperties[propertyIndex].Equals(startIndex));
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
        }

        private InfoForTask info;

        private async Task LoadAll2(int startIndex)
        {
            isLoaded = false;
            int maxIndex = NowPlayingPlaylistManager.Current.songs.Count - 1;
            int currentIndex = CurrentSongIndex;
            info = null;
            if (maxIndex <= 100)
            {
                await BasicLoad(0, maxIndex);
                isLoaded = true;
            }
            else
            {
                int endBasicLoad = (currentIndex + 50 < maxIndex) ? currentIndex + 50 : maxIndex;
                if (currentIndex < 75)
                {
                    await BasicLoad(0, 100);
                }
                else
                {
                    await BasicLoad(currentIndex - 50, endBasicLoad);
                }
                info = new InfoForTask() { c = currentIndex, max = maxIndex, e = endBasicLoad, token = cts.Token };
            }
        }

        private async Task LoadRest(InfoForTask info)
        {
            if (info == null) return;
            int currentIndex = info.c;
            int maxIndex = info.max;
            int endBasicLoad = info.e;
            CancellationToken token = info.token;
            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            try
            {
                if (currentIndex < 75)
                {
                    await StartLoadingEnd(101, maxIndex, token);
                }
                else
                {
                    await StartLoadingBeginning(0, currentIndex - 51, cts.Token);
                    if (endBasicLoad < maxIndex)
                    {
                        await StartLoadingEnd(endBasicLoad + 1, maxIndex, token);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {

            }
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
            isLoaded = true;
        }

        private async Task BasicLoad(int startIndex, int endIndex)
        {
            Debug.WriteLine("BasicLoad {0} {1}", startIndex, endIndex);
            for (int i = startIndex; i <= endIndex; i++)
            {
                var song = NowPlayingPlaylistManager.Current.songs[i];
                var playbackItem = await PlaybackItemBuilder.PreparePlaybackItem(song);
                playbackItem.Source.CustomProperties[propertyIndex] = i;
                mediaList.Items.Add(playbackItem);
            }
        }

        private async Task StartLoadingBeginning(int startIndex, int endIndex, CancellationToken token)
        {
            Debug.WriteLine("StartLoadingBeginning {0} {1}", startIndex, endIndex);
            for (int i = startIndex; i <= endIndex; i++)
            {
                var song = NowPlayingPlaylistManager.Current.songs[i];
                var playbackItem = await PlaybackItemBuilder.PreparePlaybackItem(song);
                playbackItem.Source.CustomProperties[propertyIndex] = i;
                token.ThrowIfCancellationRequested();
                mediaList.Items.Insert(i, playbackItem);
            }
        }

        private async Task StartLoadingEnd(int startIndex, int endIndex, CancellationToken token)
        {
            Debug.WriteLine("StartLoadingEnd {0} {1}", startIndex, endIndex);
            for (int i = startIndex; i <= endIndex; i++)
            {
                var song = NowPlayingPlaylistManager.Current.songs[i];
                var playbackItem = await PlaybackItemBuilder.PreparePlaybackItem(song);
                playbackItem.Source.CustomProperties[propertyIndex] = i;
                token.ThrowIfCancellationRequested();
                mediaList.Items.Add(playbackItem);
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

        private bool startPlay = false;



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
                TimeSpan delay = TimeSpan.FromMilliseconds(ms + 100);
                RadioTimer.SetTimerWithTask(delay, RefreshRadioTrackInfo);
            }
        }
        private void UpdateStats()
        {
            System.Diagnostics.Debug.WriteLine("ScrobbleTrack started at:{0} played:{1}", songStartedAt, songPlayed);
            var item = mediaList.CurrentItem;
            if (item == null)
            {
                return;
            }
            TimeSpan duration = TimeSpan.Zero;
            int a = CurrentSongIndex;
            int songId = (int)item.Source.CustomProperties[propertySongId];
            
            if (item.Source.Duration == TimeSpan.Zero)
            {
                duration = item?.Source?.Duration ?? TimeSpan.Zero;
            }
            //else
            //{
            //    duration = item?.Source?.Duration ?? TimeSpan.Zero;
            //    if (song.Duration != duration && duration != TimeSpan.Zero)
            //    {
            //        song.Duration = duration;
            //        //DatabaseManager.Current.UpdateSongDurationAsync(song.SongId, timeEnd);
            //    }
            //    duration = song.Duration;
            //}

            if (a != (int)mediaList.CurrentItemIndex)
            {

            }
            if (songPlayed.TotalSeconds >= duration.TotalSeconds*0.5 || duration.TotalSeconds >= 4 * 60)
            {
                UpdateStats2(songId, duration);
            }
        }
    }
}
