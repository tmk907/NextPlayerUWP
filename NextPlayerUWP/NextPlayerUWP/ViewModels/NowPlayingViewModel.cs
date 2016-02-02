using GalaSoft.MvvmLight.Command;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class NowPlayingViewModel : Template10.Mvvm.ViewModelBase
    {
        public NowPlayingViewModel()
        {
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
            Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            ChangeCover();
            if (PlaybackManager.Current.PlayerState == MediaPlayerState.Playing)
            {
                PlayButtonContent = Symbol.Pause;
            }
            else
            {
                PlayButtonContent = Symbol.Play;
            }
        }

        #region Properties
        private SongItem song = new SongItem();
        public SongItem Song
        {
            get {
                return song;
            }
            set {
                Set(ref song, value);
            }
        }
       
        private Symbol playButtonContent = Symbol.Play;
        public Symbol PlayButtonContent
        {
            get { return playButtonContent; }
            set { Set(ref playButtonContent, value); }
        }

        private BitmapImage cover = new BitmapImage();
        public BitmapImage Cover
        {
            get { return cover; }
            set { Set(ref cover, value); }
        }
        #endregion

        #region Commands
        private RelayCommand play;
        public RelayCommand Play
        {
            get
            {
                return play
                    ?? (play = new RelayCommand(
                    () =>
                    {
                        PlaybackManager.Current.Play();
                    }));
            }
        }

        private RelayCommand previous;
        public RelayCommand Previous
        {
            get
            {
                return previous
                    ?? (previous = new RelayCommand(
                    () =>
                    {
                        PlaybackManager.Current.Previous();
                    }));
            }
        }

        private RelayCommand next;
        public RelayCommand Next
        {
            get
            {
                return next
                    ?? (next = new RelayCommand(
                    () =>
                    {
                        PlaybackManager.Current.Next();
                    }));
            }
        }
        #endregion

        private void ChangePlayButtonContent(MediaPlayerState state)
        {
            if (state== MediaPlayerState.Playing)
            {
                PlayButtonContent = Symbol.Pause;
            }
            else
            {
                PlayButtonContent = Symbol.Play;
            }
        }

        private void ChangeSong(int index)
        {
            Song = NowPlayingPlaylistManager.Current.GetSongItem(index);
            ChangeCover();
        }

        private async Task ChangeCover()
        {
            Cover = await ImagesManager.GetCover(song.Path, false);
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged -= ChangeSong;
            Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            ChangeCover();
            //cover
            if (PlaybackManager.Current.PlayerState == MediaPlayerState.Playing)
            {
                PlayButtonContent = Symbol.Play;
            }
            else
            {
                PlayButtonContent = Symbol.Pause;
            }
            return Task.CompletedTask;
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            PlaybackManager.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged -= ChangeSong;
            if (suspending)
            {

            }
            return base.OnNavigatedFromAsync(state, suspending);
        }

    }
}
