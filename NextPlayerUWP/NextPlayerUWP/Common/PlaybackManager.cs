//using NextPlayerUWPDataLayer.Constants;
//using NextPlayerUWPDataLayer.Diagnostics;
//using NextPlayerUWPDataLayer.Enums;
//using NextPlayerUWPDataLayer.Helpers;
//using NextPlayerUWPDataLayer.Model;
//using System;
//using System.Diagnostics;
//using System.Threading;
//using Windows.Foundation;
//using Windows.Foundation.Collections;
//using Windows.Media.Playback;

//namespace NextPlayerUWP.Common
//{
//    //public delegate void MediaPlayerStateChangeHandler(MediaPlayerState state);
//    //public delegate void MediaPlayerTrackChangeHandler(int index);
//    //public delegate void MediaPlayerPositionChangeHandler(TimeSpan position, TimeSpan duration);
//    //public delegate void MediaPlayerMediaOpenHandler(TimeSpan duration);
//    //public delegate void MediaPlayerCloseHandler();
//    //public delegate void StreamUpdatedHandler(NowPlayingSong song);

//    public class PlaybackManager
//    {
//        //private static PlaybackManager instance;

//        public  PlaybackManager()
//        {
//            Logger.DebugWrite("PlaybackManager()", "");
//            backgroundAudioTaskStarted = new AutoResetEvent(false);
//            if (IsMyBackgroundTaskRunning)
//            {
//                try
//                {
//                    AddMediaPlayerEventHandlers();
//                }
//                catch (Exception ex)
//                {
//                    TelemetryAdapter.TrackEvent("PlaybackManager constructor AddMediaPlayerEventHandlers " + ex.Message);
//                }
//            }
//            App.Current.Resuming += Current_Resuming;
//            App.Current.Suspending += Current_Suspending;
//        }

//        //public static PlaybackManager Current
//        //{
//        //    get
//        //    {
//        //        if (instance == null)
//        //        {
//        //            instance = new PlaybackManager();
//        //        }
//        //        return instance;
//        //    }
//        //}
//        //private static readonly PlaybackManager current = new PlaybackManager();
//        //public static PlaybackManager Current
//        //{
//        //    get
//        //    {
//        //        return current;
//        //    }
//        //}
//        //static PlaybackManager() { }
//        //private PlaybackManager()
//        //{
//        //    backgroundAudioTaskStarted = new AutoResetEvent(false);
//        //    if (IsMyBackgroundTaskRunning)
//        //    {
//        //        try
//        //        {
//        //            AddMediaPlayerEventHandlers();
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            HockeyProxy.TrackEvent("PlaybackManager constructor AddMediaPlayerEventHandlers " + ex.Message);
//        //        }
//        //    }
//        //    //App.Current.Resuming += Current_Resuming;
//        //    //App.Current.Suspending += Current_Suspending;
//        //}

//        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
//        {
//            Logger.DebugWrite("PlaybackManager", $"Current_Suspending() {IsMyBackgroundTaskRunning}");
//            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Suspending " );
//            if (IsMyBackgroundTaskRunning)
//            {
//                SendMessageBG(AppConstants.AppState, AppConstants.AppSuspended);
//                RemoveMediaPlayerEventHandlers();
//                //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Suspending removed");
//            }
//            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
//        }

//        private void Current_Resuming(object sender, object e)
//        {
//            Logger.DebugWrite("PlaybackManager", $"Current_Resuming() {IsMyBackgroundTaskRunning}");
//            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Resuming ");
//            if (IsMyBackgroundTaskRunning)
//            {
//                SendMessageBG(AppConstants.AppState, AppConstants.AppResumed);
//                try
//                {
//                    AddMediaPlayerEventHandlers();
//                }
//                catch (Exception ex)
//                {
//                    TelemetryAdapter.TrackEvent("PlaybackManager Resuming AddMediaPlayerEventHandlers " + ex.Message);
//                }
//                //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Current_Resuming added");
//            }
//            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
//        }
//        #region Events
//        public static event MediaPlayerStateChangeHandler MediaPlayerStateChanged;
//        public void OnMediaPlayerStateChanged(MediaPlayerState state)
//        {
//            MediaPlayerStateChanged?.Invoke(state);
//        }
//        public static event MediaPlayerTrackChangeHandler MediaPlayerTrackChanged;
//        public void OnMediaPlayerTrackChanged(int index)
//        {
//             MediaPlayerTrackChanged?.Invoke(index);
//        }
//        public static event MediaPlayerPositionChangeHandler MediaPlayerPositionChanged;
//        public void OnMediaPlayerPositionChanged(TimeSpan position, TimeSpan duration)
//        {
//            MediaPlayerPositionChanged?.Invoke(position, duration);
//        }
//        public static event MediaPlayerMediaOpenHandler MediaPlayerMediaOpened;
//        public void OnMediaPlayerMediaOpened(TimeSpan duration)
//        {
//            MediaPlayerMediaOpened?.Invoke(duration);
//        }
//        public static event MediaPlayerCloseHandler MediaPlayerMediaClosed;
//        public void OnMediaPlayerMediaClosed()
//        {
//            MediaPlayerMediaClosed?.Invoke();
//        }
//        public static event StreamUpdatedHandler StreamUpdated;
//        public void OnStreamUpdated(NowPlayingSong song)
//        {
//            StreamUpdated?.Invoke(song);
//        }
//        #endregion

//        private AutoResetEvent backgroundAudioTaskStarted;
//        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

//        private int CurrentSongIndex
//        {
//            get
//            {
//                return ApplicationSettingsHelper.ReadSongIndex();
//            }
//            set
//            {
//                ApplicationSettingsHelper.SaveSongIndex(value);
//            }
//        }

//        private bool _isMyBackgroundTaskRunning = false;
//        private bool IsMyBackgroundTaskRunning
//        {
//            get
//            {
//                if (_isMyBackgroundTaskRunning)
//                    return true;
//                object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundTaskState);
//                if (value == null)
//                {
//                    return false;
//                }
//                else
//                {
//                    var state = EnumHelper.Parse<BackgroundTaskState>(value as string);
//                    _isMyBackgroundTaskRunning = state == BackgroundTaskState.Running;
//                    return _isMyBackgroundTaskRunning;
//                }
//            }
//        }

//        public MediaPlayerState PlayerState
//        {
//            get
//            {
//                if (!IsMyBackgroundTaskRunning) return MediaPlayerState.Stopped;
//                else return CurrentPlayer.CurrentState;
//            }
//        } 

//        public MediaPlayer CurrentPlayer
//        {
//            get
//            {
//                MediaPlayer mp = null;
//                int retryCount = 2;

//                while (mp == null && --retryCount >= 0)
//                {
//                    try
//                    {
//                        mp = BackgroundMediaPlayer.Current;
//                    }
//                    catch (Exception ex)
//                    {
//                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
//                        {
//                            // The foreground app uses RPC to communicate with the background process.
//                            // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
//                            // is returned when calling Current. We must restart the task, the while loop will retry to set mp.
//                            ResetAfterLostBackground();
//                            StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
//                        }
//                        else
//                        {
//                            bool success = false;
//                            try
//                            {
//                                SendMessage(AppConstants.ShutdownBGPlayer);
//                                success = true;
//                            }
//                            catch (Exception ex2)
//                            {
//                                TelemetryAdapter.TrackEvent("ShutdownBGPlayer dont work" + ex2.Message);
//                            }
//                            if (success) TelemetryAdapter.TrackEvent("ShutdownBGPlayer works");
//                            throw;
//                        }
//                    }
//                }

//                if (mp == null)
//                {
//                    throw new Exception("Failed to get a MediaPlayer instance.");
//                }

//                return mp;
//            }
//        }

//        /// <summary>
//        /// The background task did exist, but it has disappeared. Put the foreground back into an initial state. Unfortunately,
//        /// any attempts to unregister things on BackgroundMediaPlayer.Current will fail with the RPC error once the background task has been lost.
//        /// </summary>
//        private void ResetAfterLostBackground()
//        {
//            Logger.DebugWrite("PlaybackManager", "ResetAfterLostBackground");
//            BackgroundMediaPlayer.Shutdown();
//            _isMyBackgroundTaskRunning = false;
//            backgroundAudioTaskStarted.Reset();
            
//            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, BackgroundTaskState.Unknown.ToString());
//            //playButton.Content = "| |";

//            try
//            {
//                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
//            }
//            catch (Exception ex)
//            {
//                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
//                {
//                    throw new Exception("Failed to get a MediaPlayer instance. ResetAfterLostBackground");
//                }
//                else
//                {
//                    throw;
//                }
//            }
//        }

//        private void StartBackgroundAudioTask(string s, object o)
//        {
//            Logger.DebugWrite("PlaybackManager", "StartBackgroundTask");
//            AddMediaPlayerEventHandlers();

//            var startResult = Template10.Common.DispatcherWrapper.Current().DispatchAsync(() =>
//            {
//                bool result = backgroundAudioTaskStarted.WaitOne(3000);
//                //Send message to initiate playback
//                if (result == true)
//                {
//                    SendMessageBG(s, o);
//                }
//                else
//                {
//                    //ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());

//                    throw new Exception("Background Audio Task didn't start in expected time");
//                }
//            });
//            //startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
//        }

//        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
//        {
//            if (status == AsyncStatus.Completed)
//            {
//                Debug.WriteLine("Background Audio Task initialized");
//            }
//            else if (status == AsyncStatus.Error)
//            {
//                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
//            }
//        }

//        /// <summary>
//        /// Unsubscribes to MediaPlayer events. Should run only on suspend
//        /// </summary>
//        private void RemoveMediaPlayerEventHandlers()
//        {
//            try
//            {
//                BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
//                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
//            }
//            catch (Exception ex)
//            {
//                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
//                {
//                    // do nothing
//                    TelemetryAdapter.TrackEventException("RemoveMediaPlayerEventHandlers " + ex.Message + Environment.NewLine + ex.StackTrace);
//                }
//                else
//                {
//                    //throw;
//                    TelemetryAdapter.TrackEventException("RemoveMediaPlayerEventHandlers else" + ex.Message + Environment.NewLine + ex.StackTrace);
//                }
//            }
//        }

//        /// <summary>
//        /// Subscribes to MediaPlayer events
//        /// </summary>
//        private void AddMediaPlayerEventHandlers()
//        {
//            CurrentPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;

//            try
//            {
//                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
//            }
//            catch (Exception ex)
//            {
//                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
//                {
//                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
//                    ResetAfterLostBackground();
//                }
//                else
//                {
//                    //throw;
//                    TelemetryAdapter.TrackEventException("AddMediaPlayerEventHandlers else " + ex.Message + Environment.NewLine + ex.StackTrace);
//                }
//            }
//        }

//        private void UpdateTransportControls(MediaPlayerState state)
//        {
//            OnMediaPlayerStateChanged(state);
//        }

//        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
//        {
//            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
//            Template10.Common.DispatcherWrapper.Current().Dispatch(() =>
//            {
//                //UpdateTransportControls(currentState);
//                OnMediaPlayerStateChanged(currentState);
//            });
//        }

//        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
//        {
//            foreach (string key in e.Data.Keys)
//            {
//                Logger.DebugWrite("PlaybackManager", $"MessageReceivedFromBackground {key}");
//                switch (key)
//                {
//                    case AppConstants.SongIndex:

//                        Template10.Common.DispatcherWrapper.Current().Dispatch(() =>
//                        {
//                            int index = Int32.Parse(e.Data[key].ToString());
//                            CurrentSongIndex = index;
//                            OnMediaPlayerTrackChanged(index);
//                        });
//                        break;
//                    case AppConstants.MediaOpened:
//                        Template10.Common.DispatcherWrapper.Current().Dispatch(() =>
//                        {
//                            OnMediaPlayerMediaOpened(CurrentPlayer.NaturalDuration);
//                        });
//                        break;
//                    case AppConstants.Position:
//                        Template10.Common.DispatcherWrapper.Current().Dispatch(() =>
//                        {
//                            TimeSpan result = TimeSpan.Zero;
//                            TimeSpan.TryParse(e.Data[key].ToString(), out result);
//                            OnMediaPlayerPositionChanged(result, CurrentPlayer.NaturalDuration);
//                        });
//                        break;
//                    case AppConstants.PlayerClosed:
//                        Template10.Common.DispatcherWrapper.Current().Dispatch(() =>
//                        {
//                            OnMediaPlayerMediaClosed();
//                        });
//                        break;
//                    case AppConstants.StreamUpdated:
//                        Template10.Common.DispatcherWrapper.Current().Dispatch(() =>
//                        {
//                            string serialized =e.Data[key].ToString();
//                            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<NowPlayingSong>(serialized);
//                            OnStreamUpdated(deserialized);
//                        });
//                        break;
//                    case AppConstants.BackgroundTaskStarted:
//                        backgroundAudioTaskStarted.Set();
//                        break;
//                    default:
//                        break;
//                }
//            }
//        }

//        public void Previous()
//        {
//            Logger.DebugWrite("PlaybackManager", $"Previous {IsMyBackgroundTaskRunning}");
//            if (IsMyBackgroundTaskRunning)
//            {
//                SendMessage(AppConstants.SkipPrevious);
//            }
//            else
//            {
//                StartBackgroundAudioTask(AppConstants.SkipPrevious, "");
//            }
//        }

//        public void Play()
//        {
//            if (IsMyBackgroundTaskRunning)
//            {
//                Logger.DebugWrite("PlaybackManager", $"Play {IsMyBackgroundTaskRunning} {CurrentPlayer.CurrentState}");
//                if (MediaPlayerState.Playing == CurrentPlayer.CurrentState)
//                {
//                    SendMessage(AppConstants.Pause);
//                }
//                else if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
//                {
//                    SendMessage(AppConstants.Play);
//                }
//                else if (MediaPlayerState.Closed == CurrentPlayer.CurrentState)
//                {
//                    //SendMessage(AppConstants.Play);
//                    StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
//                    //SendMessageBG(AppConstants.StartPlayback, CurrentSongIndex);
//                    OnMediaPlayerTrackChanged(CurrentSongIndex);
                    
//                }
//            }
//            else
//            {
//                Logger.DebugWrite("PlaybackManager", $"Play {IsMyBackgroundTaskRunning} {MediaPlayerState.Stopped}");
//                OnMediaPlayerTrackChanged(CurrentSongIndex);
//                StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
//            }
//        }

//        public void PlayNew()
//        {
//            Logger.DebugWrite("PlaybackManager", $"PlayNew() {IsMyBackgroundTaskRunning}");
//            OnMediaPlayerTrackChanged(CurrentSongIndex);
//            if (IsMyBackgroundTaskRunning)
//            {
//                SendMessageBG(AppConstants.StartPlayback, CurrentSongIndex);
//            }
//            else
//            {
//                StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
//            }
//        }

//        public void Next()
//        {
//            Logger.DebugWrite("PlaybackManager", $"Next() {IsMyBackgroundTaskRunning}");
//            if (IsMyBackgroundTaskRunning)
//            {
//                SendMessage(AppConstants.SkipNext);
//            }
//            else
//            {
//                StartBackgroundAudioTask(AppConstants.SkipNext, "");
//            }
//        }

//        private void SendMessage(string constants)// SendMessage(constants,"")
//        {
//            var value = new ValueSet();
//            value.Add(constants, "");
//            BackgroundMediaPlayer.SendMessageToBackground(value);
//        }

//        private void SendMessageBG(string constants, object value)
//        {
//            var message = new ValueSet();
//            message.Add(constants, value);
//            BackgroundMediaPlayer.SendMessageToBackground(message);
//        }

//        public void SendMessage(string constants, object value)
//        {
//            if (IsMyBackgroundTaskRunning)
//            {
//                var message = new ValueSet();
//                message.Add(constants, value);
//                BackgroundMediaPlayer.SendMessageToBackground(message);
//            }
//        }

//        public bool IsBackgroundTaskRunning()
//        {
//            return IsMyBackgroundTaskRunning;
//        }
//    }
//}

