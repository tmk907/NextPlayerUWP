using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.System.Threading;

namespace NextPlayerUWPBackgroundAudioPlayer
{
    public enum AppState
    {
        Unknown,
        Active,
        Suspended
    }

    public enum BackgroundTaskState
    {
        Unknown,
        Started,
        Running,
        Canceled
    }

    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private SystemMediaTransportControls smtc;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private AppState foregroundAppState = AppState.Unknown;
        private NowPlayingManager nowPlayingManager;
        private bool shutdown;

        ThreadPoolTimer timer = null;
        bool timerIsSet = false;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            nowPlayingManager = new NowPlayingManager();

            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += smtc_ButtonPressed;
            smtc.PropertyChanged += smtc_PropertyChanged;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;

            shutdown = false;

            var value = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.AppState);
            if (value == null)
                foregroundAppState = AppState.Unknown;
            else
                foregroundAppState = EnumHelper.Parse<AppState>(value.ToString());

            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            if (foregroundAppState != AppState.Suspended) SendMessage(AppConstants.BackgroundTaskStarted);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());

            deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
            backgroundTaskStarted.Set();

            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled); // event may raise immediately before continung thread excecution so must be at the end
        }

        void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                // immediately set not running
                backgroundTaskStarted.Reset();

                // save state
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, Enum.GetName(typeof(AppState), foregroundAppState));
                if (shutdown)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Position, TimeSpan.Zero.ToString());
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                }

                nowPlayingManager.RemoveHandlers();
                nowPlayingManager = null;

                // unsubscribe event handlers
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                smtc.ButtonPressed -= smtc_ButtonPressed;
                smtc.PropertyChanged -= smtc_PropertyChanged;

                if (!shutdown)
                {
                    BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.ToString());
            }
            deferral.Complete(); // signals task completion. 
        }

        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            else if (sender.CurrentState == MediaPlayerState.Closed)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
        }

        async void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key.ToLower())
                {
                    case AppConstants.StartPlayback:
                        await nowPlayingManager.PlaySong(Int32.Parse(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString()));
                        UpdateUVCOnNewTrack();
                        break;
                    case AppConstants.ResumePlayback:
                        await nowPlayingManager.ResumePlayback();
                        UpdateUVCOnNewTrack();
                        break;
                    case AppConstants.SkipNext:
                        await Next();
                        break;
                    case AppConstants.SkipPrevious:
                        await Previous();
                        break;
                    case AppConstants.Play:
                        Play();
                        break;
                    case AppConstants.Pause:
                        Pause();
                        break;
                    case AppConstants.AppResumed:
                        foregroundAppState = AppState.Active;
                        nowPlayingManager.CompleteUpdate();
                        break;
                    case AppConstants.AppSuspended:
                        foregroundAppState = AppState.Suspended;
                        //ApplicationSettingsHelper.SaveSettingsValue(Constants.SongId, nowPlayingManager.GetCurrentSongId());
                        break;
                    case AppConstants.Repeat:
                        nowPlayingManager.ChangeRepeat();
                        break;
                    case AppConstants.Shuffle:
                        nowPlayingManager.ChangeShuffle();
                        break;
                    case AppConstants.Position:
                        BackgroundMediaPlayer.Current.Position = TimeSpan.Parse(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString());
                        break;
                    case AppConstants.ChangeRate:
                        nowPlayingManager.ChangeRate(Int32.Parse(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString()));
                        break;
                    case AppConstants.SetTimer:
                        SetTimer();
                        break;
                    case AppConstants.CancelTimer:
                        TimerCancel();
                        break;
                    case AppConstants.ShutdownBGPlayer:
                        ShutdownPlayer();
                        break;
                    case AppConstants.UpdateUVC:
                        UpdateUVCOnNewTrack();
                        break;
                    case AppConstants.NowPlayingListChanged:
                        nowPlayingManager.LoadPlaylist();
                        break;
                    case AppConstants.NowPlayingListRefresh:
                        string p = "";
                        try
                        {
                            p = e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString();
                            string[] s = p.Split(new string[] { "!@#$" }, StringSplitOptions.None);//s[2](artist) can equal ""
                            nowPlayingManager.UpdateSong(Int32.Parse(s[0]), s[1], s[2]);
                            UpdateUVCOnNewTrack();
                        }
                        catch (Exception ex)
                        {
                            //NextPlayerDataLayer.Diagnostics.Logger.SaveBG("NowPlayingListRefresh" + Environment.NewLine + p + Environment.NewLine + ex.Message);
                            //NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                        }
                        break;
                }
            }
        }


        private void UpdateUVCOnNewTrack()
        {
            //if (item == null)
            //{
            //    smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
            //    smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
            //    smtc.DisplayUpdater.Update();
            //    return;
            //}

            //SystemMediaTransportControlsDisplayUpdater updater = systemMediaControls.DisplayUpdater;
            //await updater.CopyFromFileAsync(MediaPlaybackType.Music, file );

            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = nowPlayingManager.GetTitle();
            smtc.DisplayUpdater.MusicProperties.Artist = nowPlayingManager.GetArtist();

            //var albumArtUri = item.Source.CustomProperties[AlbumArtKey] as Uri;
            //if (albumArtUri != null)
            //    smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
            //else
            //    smtc.DisplayUpdater.Thumbnail = null;

            smtc.DisplayUpdater.Update();
        }

        void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // If soundlevel turns to muted, app can choose to pause the music
        }

        private void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    // When the background task has been suspended and the SMTC
                    // starts it again asynchronously, some time is needed to let
                    // the task startup process in Run() complete.

                    // Wait for task to start. 
                    // Once started, this stays signaled until shutdown so it won't wait
                    // again unless it needs to.
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didnt initialize in time");

                    Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    try
                    {
                        Pause();
                    }
                    catch (Exception ex)
                    {
                        //NextPlayerDataLayer.Diagnostics.Logger.SaveBG("Audio Player HandleButtonPressed Pause" + "\n" + "Message: " + ex.Message + "\n" + "Link: " + ex.HelpLink);
                        //NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    try
                    {
                        Next();
                    }
                    catch (Exception ex)
                    {
                    }
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    try
                    {
                        Previous();
                    }
                    catch (Exception ex)
                    {
                    }
                    break;
            }
        }


        private void Play()
        {
            nowPlayingManager.Play();
        }

        private void Pause()
        {
            nowPlayingManager.Pause();
        }

        private async Task Next()
        {
            await nowPlayingManager.Next();
            UpdateUVCOnNewTrack();
        }

        private async Task Previous()
        {
            await nowPlayingManager.Previous();
            UpdateUVCOnNewTrack();
        }

        private void ShutdownPlayer()
        {
            shutdown = true;
            BackgroundMediaPlayer.Shutdown();
        }

        private void SendMessage(string message, string content = "")
        {
            var value = new ValueSet();
            value.Add(message, content);
            BackgroundMediaPlayer.SendMessageToForeground(value);
        }

        private void SetTimer()
        {
            var t = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerTime);
            long tt = 0;
            if (t != null)
            {
                tt = (long)t;
            }

            TimeSpan currentTime = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute) + TimeSpan.FromSeconds(DateTime.Now.Second);
            long currentTimeTicks = currentTime.Ticks;

            TimeSpan delay = TimeSpan.FromTicks(tt - currentTimeTicks);
            if (delay > TimeSpan.Zero)
            {
                if (timerIsSet)
                {
                    TimerCancel();
                }
                timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(TimerCallback), delay);
                timerIsSet = true;
            }
        }

        private void TimerCallback(ThreadPoolTimer timer)
        {
            ShutdownPlayer();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            TimerCancel();
        }

        private void TimerCancel()
        {
            timerIsSet = false;
            if (timer != null)
            {
                timer.Cancel();
            }
        }
    }
}
