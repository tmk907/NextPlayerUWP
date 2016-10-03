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
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace NextPlayerUWP.Common
{
    public delegate void MediaPlayerStateChangeHandler(MediaPlaybackState state);
    public delegate void MediaPlayerTrackChangeHandler(int index);
    public delegate void MediaPlayerMediaOpenHandler();
    public delegate void StreamUpdatedHandler(NowPlayingSong song);
    
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

        private TimeSpan timePreviousOrBeggining = TimeSpan.FromSeconds(5);

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

        bool isGaplessPlaybackReady = true;
        PlaybackTimer RadioTimer;
        PlaybackTimer MusicPlaybackTimer;

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
        }

        public async Task Initialize()
        {
            System.Diagnostics.Debug.WriteLine("PlaybackService Initialize Start");
            await LoadAll(CurrentSongIndex);
            //System.Diagnostics.Debug.WriteLine("PlaybackService Initialize Loaded");
            Player.Source = mediaList;
            canPlay = true;
            System.Diagnostics.Debug.WriteLine("PlaybackService Initialize End");
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
        #region Events

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("Player_MediaEnded {0}", sender.PlaybackSession.PlaybackState);
            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            UpdateStats();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;
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
        public static event MediaPlayerTrackChangeHandler MediaPlayerTrackChanged;
        public void OnMediaPlayerTrackChanged(int index)
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerTrackChanged {0}", index);
            MediaPlayerTrackChanged?.Invoke(index);
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

        public int CurrentSongIndex
        {
            get
            {
                return ApplicationSettingsHelper.ReadSongIndex();
            }
            private set
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
                var session = Player.PlaybackSession;
                if (session.CanSeek && value >= TimeSpan.Zero)
                {
                    if (value <= session.NaturalDuration)
                    {
                        session.Position = value;
                    }
                    else
                    {
                        session.Position = session.NaturalDuration;
                    }
                }
                else
                {

                }
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
                System.Diagnostics.Debug.WriteLine("Play() canPlay == false");
            }
        }

        public void Pause()
        {
            Player.Pause();
            songPlayed = DateTime.Now - songStartedAt + songPlayed;
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

            if (IsLast)
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

        int fastForwardInterval = 5000;
        int rewindInterval = 5000;
        int fastForwardFasterInterval = 15000;
        int rewindFasterInterval = 15000;

        public void FastForward()
        {
            System.Diagnostics.Debug.WriteLine("FastForward");
            var session = Player.PlaybackSession;
            if (session.CanSeek)
            {
                var interval = TimeSpan.FromMilliseconds(fastForwardInterval);
                if (session.Position + interval + TimeSpan.FromMilliseconds(500) < session.NaturalDuration)
                {
                    session.Position += TimeSpan.FromMilliseconds(fastForwardInterval);
                }
                else
                {
                    Pause();
                    session.Position = TimeSpan.Zero;
                }
            }
        }

        public void Rewind()
        {
            System.Diagnostics.Debug.WriteLine("Rewind");
            var session = Player.PlaybackSession;
            if (session.CanSeek)
            {
                if (session.Position > TimeSpan.FromMilliseconds(rewindInterval))
                {
                    session.Position -= TimeSpan.FromMilliseconds(rewindInterval);
                }
                else
                {
                    Pause();
                    session.Position = TimeSpan.Zero;
                }
            }
        }

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
                case MediaPlaybackState.Buffering:
                    if (Player.PlaybackSession.CanPause)
                    {
                        Pause();
                    }
                    break;
                case MediaPlaybackState.Opening:
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

        public int PlaybackRatePercent
        {
            get 
            {
                return (int)Player.PlaybackSession.PlaybackRate * 100;
            }
            set
            {
                if (value>30 && value < 400)
                {
                    double playbackRate = value / 100.0;
                    Player.PlaybackSession.PlaybackRate = playbackRate;
                }
            }
        }

        public void ResetPlaybackRate()
        {
            var session = Player.PlaybackSession;
            session.PlaybackRate = 1.0;
        }

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

            MusicPlaybackTimer.SetTimerWithAction(delay, MusicTimerCallback);
        }

        private void MusicTimerCallback()
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            Pause();
        }

        public void CancelPlaybackTimer()
        {
            MusicPlaybackTimer.TimerCancel();
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
            int index = 0;
            //Stopwatch s1 = new Stopwatch();
            foreach(var song in NowPlayingPlaylistManager.Current.songs)
            {
                //s1.Start();
                var playbackItem = await PlaybackItemBuilder.PreparePlaybackItem(song);
                //s1.Stop();
                playbackItem.Source.CustomProperties[propertyIndex] = index;
                mediaList.Items.Add(playbackItem);
                index++;
            }
            //Debug.WriteLine("{0} {1} ", s1.ElapsedMilliseconds, PlaybackItemBuilder.time);
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

        private bool startPlay = false;
        public void JumpTo(int startIndex)//nie zmienia stanu na playing!!
        {
            System.Diagnostics.Debug.WriteLine("JumpTo {0}", startIndex);
            if (!canPlay) return;
            command = true;

            songPlayed = DateTime.Now - songStartedAt + songPlayed;
            UpdateStats();
            songStartedAt = DateTime.Now;
            songPlayed = TimeSpan.Zero;

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
            command = true;
            
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
        }

        private static async Task<MediaPlaybackItem> PreparePlaybackItem(SongItem song)
        {
            return await PlaybackItemBuilder.PreparePlaybackItem(song);
        }

        public static void Source_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("UpdateMediaList {0} {1}", sender.IsOpen, (args.Error?.ExtendedError.ToString()) ?? "");
            if (sender.IsOpen)
            {
                Instance.OnMediaPlayerMediaOpened();
            }
            //if (sender.State == MediaSourceState.Failed)
            //{
            //    sender.Reset();
            //}
        }

        public static void RadioSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("UpdateMediaList {0}", sender.IsOpen);
            if (sender.IsOpen)
            {
                Instance.RadioTimer.SetTimerWithTask(TimeSpan.FromMilliseconds(500), Instance.RefreshRadioTrackInfo);
            }
        }

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

        public async Task UpdateSongStatistics(int songId, TimeSpan totalDuration)
        {
            if (songId > 0)
            {
                await DatabaseManager.Current.UpdateSongStatistics(songId);
            }
            else
            {
                //log error
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
            var duration = item?.Source.Duration ?? TimeSpan.Zero;
            int a = (int)mediaList.CurrentItemIndex;
            if (a != CurrentSongIndex)
            {

            }
            int songId = (int)item.Source.CustomProperties[propertySongId];
            UpdateStats2(songId, duration, songPlayed);
        }

        private async Task UpdateStats2(int songId, TimeSpan duration, TimeSpan songPlayed)
        {
            var song = await DatabaseManager.Current.GetSongItemAsync(songId);

            if (CanFlagSongAsPlayed(songPlayed) && (song.SourceType == MusicSource.LocalFile || song.SourceType == MusicSource.LocalNotMusicLibrary))
            {
                await UpdateSongStatistics(song.SongId, duration);
                await ScrobbleTrack(song, duration);
            }
        }

        private async Task ScrobbleTrack(SongItem song, TimeSpan duration)
        {
            if (duration > TimeSpan.FromSeconds(30) && lastFmCache.AreCredentialsSet())
            {
                await CacheTrackScrobble(song);
            }
        }

        private async Task CacheTrackScrobble(SongItem song)
        {
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
