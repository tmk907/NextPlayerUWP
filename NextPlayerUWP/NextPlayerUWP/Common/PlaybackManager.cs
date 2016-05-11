﻿using GalaSoft.MvvmLight.Threading;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace NextPlayerUWP.Common
{
    public delegate void MediaPlayerStateChangeHandler(MediaPlayerState state);
    public delegate void MediaPlayerTrackChangeHandler(int index);
    public delegate void MediaPlayerPositionChangeHandler(TimeSpan position, TimeSpan duration);
    public delegate void MediaPlayerMediaOpenHandler(TimeSpan duration);
    public delegate void MediaPlayerCloseHandler();
    public delegate void StreamUpdatedHandler(NowPlayingSong song);

    public sealed class PlaybackManager
    {
        private static readonly PlaybackManager current = new PlaybackManager();
        public static PlaybackManager Current
        {
            get
            {
                return current;
            }
        }
        static PlaybackManager() { }
        private PlaybackManager()
        {
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("PlaybackManager ");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            backgroundAudioTaskStarted = new AutoResetEvent(false);
            if (IsMyBackgroundTaskRunning)
            {
                try
                {
                    AddMediaPlayerEventHandlers();
                }
                catch(Exception ex)
                {
                    HockeyProxy.TrackEvent("PlaybackManager constructor AddMediaPlayerEventHandlers " + ex.Message);
                }
            }
            App.Current.Resuming += Current_Resuming;
            App.Current.Suspending += Current_Suspending;
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Suspending " );
            if (IsMyBackgroundTaskRunning)
            {
                PlaybackManager.Current.SendMessageBG(AppConstants.AppState, AppConstants.AppSuspended);
                RemoveMediaPlayerEventHandlers();
                //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Suspending removed");
            }
            NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
        }

        private void Current_Resuming(object sender, object e)
        {
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Resuming ");
            if (IsMyBackgroundTaskRunning)
            {
                try
                {
                    AddMediaPlayerEventHandlers();
                }
                catch (Exception ex)
                {
                    HockeyProxy.TrackEvent("PlaybackManager Resuming AddMediaPlayerEventHandlers " + ex.Message);
                }
                PlaybackManager.Current.SendMessageBG(AppConstants.AppState, AppConstants.AppResumed);
                //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Resuming added");
            }
            NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
        }

        public static event MediaPlayerStateChangeHandler MediaPlayerStateChanged;
        public void OnMediaPlayerStateChanged(MediaPlayerState state)
        {
            MediaPlayerStateChanged?.Invoke(state);
        }
        public static event MediaPlayerTrackChangeHandler MediaPlayerTrackChanged;
        public void OnMediaPlayerTrackChanged(int index)
        {
             MediaPlayerTrackChanged?.Invoke(index);
        }
        public static event MediaPlayerPositionChangeHandler MediaPlayerPositionChanged;
        public void OnMediaPlayerPositionChanged(TimeSpan position, TimeSpan duration)
        {
            MediaPlayerPositionChanged?.Invoke(position, duration);
        }
        public static event MediaPlayerMediaOpenHandler MediaPlayerMediaOpened;
        public void OnMediaPlayerMediaOpened(TimeSpan duration)
        {
            MediaPlayerMediaOpened?.Invoke(duration);
        }
        public static event MediaPlayerCloseHandler MediaPlayerMediaClosed;
        public void OnMediaPlayerMediaClosed()
        {
            MediaPlayerMediaClosed?.Invoke();
        }
        public static event StreamUpdatedHandler StreamUpdated;
        public void OnStreamUpdated(NowPlayingSong song)
        {
            StreamUpdated?.Invoke(song);
        }

        private AutoResetEvent backgroundAudioTaskStarted;
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

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

        private bool _isMyBackgroundTaskRunning = false;
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (_isMyBackgroundTaskRunning)
                    return true;
                object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                else
                {
                    var state = EnumHelper.Parse<BackgroundTaskState>(value as string);
                    _isMyBackgroundTaskRunning = state == BackgroundTaskState.Running;
                    return _isMyBackgroundTaskRunning;
                }
            }
        }

        public MediaPlayerState PlayerState
        {
            get
            {
                if (!IsMyBackgroundTaskRunning) return MediaPlayerState.Stopped;
                else return CurrentPlayer.CurrentState;
            }
        } 

        public MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

                while (mp == null && --retryCount >= 0)
                {
                    try
                    {
                        mp = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            // The foreground app uses RPC to communicate with the background process.
                            // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
                            // is returned when calling Current. We must restart the task, the while loop will retry to set mp.
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                        }
                        else
                        {
                            bool success = false;
                            try
                            {
                                SendMessage(AppConstants.ShutdownBGPlayer);
                                success = true;
                            }
                            catch (Exception ex2)
                            {
                                HockeyProxy.TrackEvent("ShutdownBGPlayer dont work" + ex2.Message);
                            }
                            if (success) HockeyProxy.TrackEvent("ShutdownBGPlayer works");
                            throw;
                        }
                    }
                }

                if (mp == null)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }

                return mp;
            }
        }

        /// <summary>
        /// The background task did exist, but it has disappeared. Put the foreground back into an initial state. Unfortunately,
        /// any attempts to unregister things on BackgroundMediaPlayer.Current will fail with the RPC error once the background task has been lost.
        /// </summary>
        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _isMyBackgroundTaskRunning = false;
            backgroundAudioTaskStarted.Reset();
            
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, BackgroundTaskState.Unknown.ToString());
            //playButton.Content = "| |";

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    throw new Exception("Failed to get a MediaPlayer instance. ResetAfterLostBackground");
                }
                else
                {
                    throw;
                }
            }
        }

        private void StartBackgroundAudioTask(string s, object o)
        {
            AddMediaPlayerEventHandlers();

            var startResult = DispatcherHelper.RunAsync(() =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    SendMessageBG(s, o);
                }
                else
                {
                    //ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());

                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            });
            startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                Debug.WriteLine("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
            }
        }

        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // do nothing
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
                    ResetAfterLostBackground();
                }
                else
                {
                    throw;
                }
            }
        }

        private void UpdateTransportControls(MediaPlayerState state)
        {
            OnMediaPlayerStateChanged(state);
            //switch (state)
            //{
            //    case MediaPlayerState.Playing:
            //        //PlayButtonContent = "\uE17e\uE103";// Change to pause button
            //        break;
            //    case MediaPlayerState.Paused:
            //        //PlayButtonContent = "\uE17e\uE102";     // Change to play button
            //        break;
            //}
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                UpdateTransportControls(currentState);

            });
            }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case AppConstants.SongIndex:
                        
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            int index = Int32.Parse(e.Data[key].ToString());
                            CurrentSongIndex = index;
                            OnMediaPlayerTrackChanged(index);
                        });
                        break;
                    case AppConstants.MediaOpened:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            OnMediaPlayerMediaOpened(CurrentPlayer.NaturalDuration);
                            //TimeSpan t = BackgroundMediaPlayer.Current.NaturalDuration;
                            //double absvalue = (int)Math.Round(t.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
                            //ProgressBarMaxValue = absvalue;
                            //ProgressBarValue = 0.0;
                            //CurrentTime = TimeSpan.Zero;
                            //EndTime = BackgroundMediaPlayer.Current.NaturalDuration;
                            //PlaybackRate = BackgroundMediaPlayer.Current.PlaybackRate * 100.0;
                            //SaveCached();
                        });
                        break;
                    case AppConstants.Position:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            TimeSpan result = TimeSpan.Zero;
                            TimeSpan.TryParse(e.Data[key].ToString(), out result);
                            OnMediaPlayerPositionChanged(result, CurrentPlayer.NaturalDuration);
                        });
                        break;
                    case AppConstants.PlayerClosed:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            OnMediaPlayerMediaClosed();
                        });
                        break;
                    case AppConstants.StreamUpdated:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            string serialized =e.Data[key].ToString();
                            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<NowPlayingSong>(serialized);
                            OnStreamUpdated(deserialized);
                        });
                        break;
                    case AppConstants.BackgroundTaskStarted:
                        backgroundAudioTaskStarted.Set();
                        break;
                    default:
                        break;
                }
            }
        }

        public void Previous()
        {
            if (IsMyBackgroundTaskRunning)
            {
                SendMessage(AppConstants.SkipPrevious);
            }
            else
            {
                StartBackgroundAudioTask(AppConstants.SkipPrevious, "");
            }
        }

        public void Play()
        {
            if (IsMyBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == CurrentPlayer.CurrentState)
                {
                    SendMessage(AppConstants.Pause);
                }
                else if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
                {
                    SendMessage(AppConstants.Play);
                }
                else if (MediaPlayerState.Closed == CurrentPlayer.CurrentState)
                {
                    OnMediaPlayerTrackChanged(CurrentSongIndex);
                    StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                }
            }
            else
            {
                OnMediaPlayerTrackChanged(CurrentSongIndex);
                StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
            }
        }

        public void PlayNew()
        {
            OnMediaPlayerTrackChanged(CurrentSongIndex);
            if (IsMyBackgroundTaskRunning)
            {
                SendMessageBG(AppConstants.StartPlayback, CurrentSongIndex);
            }
            else
            {
                StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
            }
        }

        public void Next()
        {
            if (IsMyBackgroundTaskRunning)
            {
                SendMessage(AppConstants.SkipNext);
            }
            else
            {
                StartBackgroundAudioTask(AppConstants.SkipNext, "");
            }
        }

        private void SendMessage(string constants)// SendMessage(constants,"")
        {
            var value = new ValueSet();
            value.Add(constants, "");
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public void SendMessage(string constants, object value)
        {
            if (IsMyBackgroundTaskRunning)
            {
                var message = new ValueSet();
                message.Add(constants, value);
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
        }

        private void SendMessageBG(string constants, object value)
        {
            var message = new ValueSet();
            message.Add(constants, value);
            BackgroundMediaPlayer.SendMessageToBackground(message);
        }

        public bool IsBackgroundTaskRunning()
        {
            return IsMyBackgroundTaskRunning;
        }
    }
}

