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
        JamendoRadiosData jRadioData;
        private LastFmCache lastFmCache;

        private const int maxSongsNumber = 5;
        private const int playingSongIndex = 2;

        /// <summary>
        /// The data model of the active playlist. An application might
        /// have a database of items representing a user's media library,
        /// but here we use a simple list loaded from a JSON asset.
        /// </summary>
        //public MediaList CurrentPlaylist { get; set; }

        MediaPlaybackList mediaList;

        public PlaybackService()
        {
            Logger.DebugWrite("PaybackService","");
            
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

        }

        #region Events

        private async void MediaList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (!command)//song finished
            {
                if (args.NewItem != null && args.OldItem != null)
                {
                    await Next(false);
                }
            }
            command = false;
        }

        private async void Player_MediaEnded(MediaPlayer sender, object args)
        {
            //await Next(false);
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
            OnMediaPlayerStateChanged(sender.PlaybackState);
        }


        
        public static event MediaPlayerStateChangeHandler MediaPlayerStateChanged;
        public void OnMediaPlayerStateChanged(MediaPlaybackState state)
        {
            MediaPlayerStateChanged?.Invoke(state);
        }
        public static event MediaPlayerTrackChangeHandler MediaPlayerTrackChanged;
        public void OnMediaPlayerTrackChanged(int index)
        {
            MediaPlayerTrackChanged?.Invoke(index);
        }
        public static event MediaPlayerMediaOpenHandler MediaPlayerMediaOpened;
        public void OnMediaPlayerMediaOpened()
        {
            System.Diagnostics.Debug.WriteLine("OnMediaPlayerMediaOpened {0}");
            MediaPlayerMediaOpened?.Invoke();
        }
        public static event StreamUpdatedHandler StreamUpdated;
        public void OnStreamUpdated(NowPlayingSong song)
        {
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
                OnMediaPlayerTrackChanged(value);
                ApplicationSettingsHelper.SaveSongIndex(value);
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

        public async Task JumpTo(int startIndex)
        {
            System.Diagnostics.Debug.WriteLine("JumpTo {0}", startIndex);
            command = true;
            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            Player.Pause();
            CurrentSongIndex = startIndex;
            foreach (var item in mediaList.Items)
            {
                item.Source.Reset();
            }
            mediaList.Items.Clear();
            if (NowPlayingPlaylistManager.Current.songs.Count <= maxSongsNumber)
            {
                for (int i = 0; i < NowPlayingPlaylistManager.Current.songs.Count; i++)
                {
                    var song = NowPlayingPlaylistManager.Current.songs[i];
                    var playbackItem = await PreparePlaybackItem(song);
                    playbackItem.Source.CustomProperties["index"] = i;
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
                    playbackItem.Source.CustomProperties["index"] = index;
                    mediaList.Items.Add(playbackItem);
                }
            }
            mediaList.StartingItem = mediaList.Items[playingSongIndex];
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
            Player.Source = mediaList;

            Player.Play();
            OnMediaPlayerMediaOpened();
        }

        public async Task PlayNewList(int startIndex)
        {
            System.Diagnostics.Debug.WriteLine("PlayNewList {0}", startIndex);
            command = true;
            shuffle = false;
            mediaList.CurrentItemChanged -= MediaList_CurrentItemChanged;
            Player.Pause();
            CurrentSongIndex = startIndex;
            foreach(var item in mediaList.Items)
            {
                item.Source.Reset();
            }
            mediaList.Items.Clear();

            //foreach(var song in NowPlayingPlaylistManager.Current.songs)
            //{
            //    var pi = await PreparePlaybackItem(song);
            //    mediaList.Items.Add(pi);
            //}

            if (NowPlayingPlaylistManager.Current.songs.Count <= maxSongsNumber)
            {
                for (int i = 0; i < NowPlayingPlaylistManager.Current.songs.Count; i++)
                {
                    var song = NowPlayingPlaylistManager.Current.songs[i];
                    var playbackItem = await PreparePlaybackItem(song);
                    playbackItem.Source.CustomProperties["index"] = i;
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
                    playbackItem.Source.CustomProperties["index"] = index;
                    mediaList.Items.Add(playbackItem);
                }
            }
            mediaList.StartingItem = mediaList.Items[playingSongIndex];
            mediaList.CurrentItemChanged += MediaList_CurrentItemChanged;
            Player.Source = mediaList;

            Player.Play();
            OnMediaPlayerMediaOpened();
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

                    int index = (int)mediaList.Items[0].Source.CustomProperties["index"];
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
                    playbackItem.Source.CustomProperties["index"] = index;
                    
                    mediaList.Items.Insert(0, playbackItem);
                    //foreach (var item in mediaList.Items)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Item index{0}", item.Source.CustomProperties["index"]);
                    //}

                }
                else
                {
                    mediaList.Items[0].Source.Reset();
                    mediaList.Items.RemoveAt(0);
                    int index = (int)mediaList.Items[mediaList.Items.Count - 1].Source.CustomProperties["index"];
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
                    playbackItem.Source.CustomProperties["index"] = index;
                    mediaList.Items.Add(playbackItem);
                    //foreach (var item in mediaList.Items)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Item index{0}", item.Source.CustomProperties["index"]);
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
            switch (song.SourceType)
            {
                case MusicSource.LocalFile:
                    StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                    var source = MediaSource.CreateFromStorageFile(file);
                    var playbackItem = new MediaPlaybackItem(source);
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
                    source.CustomProperties["songid"] = song.SongId;
                    return playbackItem;
                default:
                    return null;
            }
        }

        public void Play()
        {
            Player.Play();
        }

        public void Pause()
        {
            Player.Pause();
        }

        private bool command = false;
        public async Task Next(bool userChoice = true)
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
                        await UpdateMediaList();
                        CurrentSongIndex = 0;
                    }
                    else
                    {
                        mediaList.MovePrevious();
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
                    await UpdateMediaList(false);
                    CurrentSongIndex++;
                }
            }
            else if (repeat == RepeatEnum.RepeatOnce)
            {
                if (isSongRepeated)
                {
                    isSongRepeated = false;
                    if (IsLast)
                    {
                        if (userChoice)
                        {
                            mediaList.MoveNext();
                            await UpdateMediaList();
                            CurrentSongIndex = 0;
                        }
                        else
                        {
                            mediaList.MovePrevious();
                        }
                    }
                    else
                    {
                        if (userChoice)
                        {
                            mediaList.MoveNext();
                        }
                        await UpdateMediaList(false);
                        CurrentSongIndex++;
                    }
                }
                else
                {
                    if (userChoice)
                    {
                        mediaList.MoveNext();
                        await UpdateMediaList();
                        if (IsLast)
                        {
                            CurrentSongIndex = 0;
                        }
                        else
                        {
                            CurrentSongIndex++;
                        }
                    }
                    else
                    {
                        mediaList.MovePrevious();
                        isSongRepeated = true;
                    }
                }
            }
            else if (repeat == RepeatEnum.RepeatPlaylist)
            {
                if (userChoice)
                {
                    mediaList.MoveNext();
                }
                await UpdateMediaList();
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

        public async Task Previous()
        {
            command = true;
            System.Diagnostics.Debug.WriteLine("Previous");
            var session = Player.PlaybackSession;
            if (session.Position > timePreviousOrBeggining)
            {
                session.Position = TimeSpan.Zero;
            }
            else
            {
                isSongRepeated = false;
                
                mediaList.MovePrevious();
                await UpdateMediaList(true);
                CurrentSongIndex--;
                if (CurrentSongIndex < 0) CurrentSongIndex = NowPlayingPlaylistManager.Current.songs.Count - 1;
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
    }
}
