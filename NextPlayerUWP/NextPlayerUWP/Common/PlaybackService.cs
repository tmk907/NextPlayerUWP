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

namespace NextPlayerUWP.Common
{
    public delegate void MediaPlayerStateChangeHandler(MediaPlaybackState state);
    public delegate void MediaPlayerTrackChangeHandler(int index);
    public delegate void MediaPlayerMediaOpenHandler(TimeSpan duration);
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

        private TimeSpan timePreviousOrBeggining = TimeSpan.FromSeconds(5);

        private AppState foregroundAppState = AppState.Unknown;
        JamendoRadiosData jRadioData;
        private LastFmCache lastFmCache;

        /// <summary>
        /// The data model of the active playlist. An application might
        /// have a database of items representing a user's media library,
        /// but here we use a simple list loaded from a JSON asset.
        /// </summary>
        //public MediaList CurrentPlaylist { get; set; }

        MediaPlaybackList mediaList;

        public PlaybackService()
        {
            Logger.DebugWrite("PaybackService", "");
            
            // Create the player instance
            Player = new MediaPlayer();
            Player.AutoPlay = false;
            Player.MediaOpened += Player_MediaOpened;
            Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

            mediaList = new MediaPlaybackList();
            mediaList.CurrentItemChanged += Mlist_CurrentItemChanged;
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            OnMediaPlayerStateChanged(sender.PlaybackState);
        }

        private void Player_MediaOpened(MediaPlayer sender, object args)
        {
            OnMediaPlayerMediaOpened(Player.PlaybackSession.NaturalDuration);
        }

        private void Mlist_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            CurrentSongIndex = (int)sender.CurrentItemIndex;
            OnMediaPlayerTrackChanged((int)sender.CurrentItemIndex);
            UpdateTile();
        }

        #region Events
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
        public void OnMediaPlayerMediaOpened(TimeSpan duration)
        {
            MediaPlayerMediaOpened?.Invoke(duration);
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
                ApplicationSettingsHelper.SaveSongIndex(value);
            }
        }

        public MediaPlaybackState PlayerState
        {
            get
            {
                return Player.PlaybackSession.PlaybackState;
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

        public async Task NewPlaylists(IEnumerable<SongItem> playlist)
        {
            var currentIndex = ApplicationSettingsHelper.ReadSongIndex();
            if (currentIndex >= playlist.Count())
            {
                currentIndex = playlist.Count() - 1;
            }
            Player.Pause();
            mediaList.Items.Clear();
            foreach (var item in playlist)
            {
                switch (item.SourceType)
                {
                    case MusicSource.LocalFile:
                        StorageFile file = await StorageFile.GetFileFromPathAsync(item.Path);
                        var source = MediaSource.CreateFromStorageFile(file);
                        var playbackItem = new MediaPlaybackItem(source);
                        var displayProperties = playbackItem.GetDisplayProperties();
                        displayProperties.Type = Windows.Media.MediaPlaybackType.Music;
                        displayProperties.MusicProperties.Artist = item.Artist;
                        displayProperties.MusicProperties.AlbumTitle = item.Album;
                        displayProperties.MusicProperties.Title = item.Title;
                        try
                        {
                            displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(item.AlbumArtUri);
                        }
                        catch (Exception ex)
                        {
                            displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
                        }
                        playbackItem.ApplyDisplayProperties(displayProperties);
                        source.CustomProperties["songid"] = item.SongId;
                        mediaList.Items.Add(playbackItem);
                        break;
                    default:
                        break;
                }
            }
            mediaList.StartingItem = mediaList.Items[currentIndex];
            Player.Source = mediaList;
        }

        public void Play()
        {
            Player.Play();
            
        }

        public void PlayNew()
        {
            OnMediaPlayerTrackChanged(CurrentSongIndex);
            mediaList.MoveTo((uint)CurrentSongIndex);
            Player.Play();
        }

        public void Pause()
        {
            Player.Pause();
           
        }

        public async Task Next(bool userchoice = true)
        {
            mediaList.MoveNext();
        }

        public async Task Previous()
        {
            var session = Player.PlaybackSession;
            if (session.Position > timePreviousOrBeggining)
            {
                session.Position = TimeSpan.Zero;
            }
            else
            {
                mediaList.MovePrevious();
            }
        }

        public void Shuffle()
        {
            mediaList.ShuffleEnabled = !mediaList.ShuffleEnabled;
        }

        public void ChangeRepeat()
        {

        }

        public void ChangeVolume(int volume)
        {
            //if (volume>=0 && volume <= 100) { }
            Player.Volume = volume;
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
