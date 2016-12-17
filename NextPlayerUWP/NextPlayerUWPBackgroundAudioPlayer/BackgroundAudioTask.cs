﻿//using FFmpegInterop;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace NextPlayerUWPBackgroundAudioPlayer
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private SystemMediaTransportControls smtc;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private AppState foregroundAppState = AppState.Unknown;
        private NowPlayingManager nowPlayingManager;
        private bool shutdown;
        //private TileUpdater myTileUpdater;    

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Logger.DebugWrite("BackgroundAudioTask", "");
            //Stopwatch s1 = new Stopwatch();
            //s1.Start();
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
            BackgroundMediaPlayer.Current.Volume = ((int)(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Volume) ?? 100)) / 100.0;

            if (foregroundAppState != AppState.Suspended) SendMessage(AppConstants.BackgroundTaskStarted);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());

            var to = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerOn);
            if (to != null && (bool)to)
            {
                //SetTimer();//!
            }

            //myTileUpdater = new TileUpdater();

            deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it
            //s1.Stop();
            //Debug.WriteLine("backgroundaudio "+ s1.ElapsedMilliseconds);
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

                NextPlayerUWPDataLayer.Diagnostics.Logger.SaveBG("OnCanceled" + "\n" + reason + "\n" + sender?.SuspendedCount);
                NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFileBG();

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
                Logger.DebugWrite("BGAudio()", "CurrentStateChanged Closed");
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
                        nowPlayingManager.UpdateForegroundState(foregroundAppState);
                        nowPlayingManager.CompleteUpdate();
                        break;
                    case AppConstants.AppSuspended:
                        foregroundAppState = AppState.Suspended;
                        nowPlayingManager.UpdateForegroundState(foregroundAppState);
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
                        nowPlayingManager.ChangePlaybackRate(Int32.Parse(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString()));
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
                            nowPlayingManager.UpdateSong(Int32.Parse(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString()));
                            UpdateUVCOnNewTrack();
                        }
                        catch (Exception ex)
                        {
                            //NextPlayerDataLayer.Diagnostics.Logger.SaveBG("NowPlayingListRefresh" + Environment.NewLine + p + Environment.NewLine + ex.Message);
                            //NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                        }
                        break;
                    case "ffmpeg-db":
                        string path = e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString();
                        await OpenUsingFFmpeg(path,false);
                        break;
                    case "ffmpeg-accesslist":
                        string path2 = e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString();
                        await OpenUsingFFmpeg(path2, true);
                        break;
                    case AppConstants.LfmLogin:
                        nowPlayingManager.RefreshLastFmCredentials();
                        break;
                    case AppConstants.Volume:
                        ChangeVolume((double)(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value));
                        break;
                    default:
                        break;
                }
            }
        }

        //private FFmpegInteropMSS FFmpegMSS;

        private async Task OpenUsingFFmpeg(string path, bool fromAccessList)
        {
            //path = @"D:\Muzyka\Jean Michel Jarre\Jean Michel Jarre - Aero [DTS]\11 - Aerology.ac3";
            //path = @"D:\Muzyka\Moja muzyka\jhg.ogg";
            StorageFile file;
            if (fromAccessList)
            {
                file = await FutureAccessHelper.GetFileFromPathAsync(path);
                if (file != null)
                {
                    
                }
                else
                {
                    BackgroundMediaPlayer.Current.Pause();
                    await Next();
                    return;
                }
            }
            else
            {
                file = await StorageFile.GetFileFromPathAsync(path);
            }

            IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);

            try
            {
                // Instantiate FFmpegInteropMSS using the opened local file stream
                //FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, true, false);
                //MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                //if (mss != null)
                //{
                //    BackgroundMediaPlayer.Current.AutoPlay = false;
                //    // Pass MediaStreamSource to Media Element
                //    BackgroundMediaPlayer.Current.SetMediaSource(mss);
                //}
                //else
                //{

                //}
            }
            catch (Exception ex)
            {

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

            string title = nowPlayingManager.GetTitle();
            string artist = nowPlayingManager.GetArtist();

            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = title;
            smtc.DisplayUpdater.MusicProperties.Artist = artist;

            string path = nowPlayingManager.GetAlbumArt();
            if (path != AppConstants.AlbumCover)
            {
                Uri coverUri;
                try
                {
                    coverUri = new Uri(path);
                }
                catch(Exception ex)
                {
                    coverUri = new Uri(AppConstants.AlbumCover);
                    NextPlayerUWPDataLayer.Diagnostics.Logger.SaveBG("UpdateUVCOnNewTrack path:" + path + Environment.NewLine + ex);
                    NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFileBG();
                }
                if (coverUri.IsFile)
                {
                    smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(coverUri);
                }
                else
                {
                    smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(coverUri);
                }
            }
            else
            {
                smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(AppConstants.AlbumCover));
            }
            //var albumArtUri = item.Source.CustomProperties[AlbumArtKey] as Uri;
            //if (albumArtUri != null)
            //    smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
            //else
            //    smtc.DisplayUpdater.Thumbnail = null;

            smtc.DisplayUpdater.Update();
            //if (path != AppConstants.AlbumCover)
            //{
            //    myTileUpdater.UpdateAppTileBG(title, artist, path);
            //}
            //else
            //{
            //    myTileUpdater.UpdateAppTileBG(title, artist, AppConstants.AppLogoMedium);
            //}
        }

        private void ChangeVolume(double volume)
        {
            BackgroundMediaPlayer.Current.Volume = volume;
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
                    //bool result = backgroundTaskStarted.WaitOne(3000);
                    //if (!result)
                    //{
                    //    NextPlayerUWPDataLayer.Diagnostics.Logger.SaveBG("smtc_ButtonPressed Background Task didnt initialize in time");
                    //    NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFileBG();
                    //    throw new Exception("Background Task didnt initialize in time");
                    //}
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
                default:
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
            BackgroundMediaPlayer.Current.Pause();
            BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
            //this.OnCanceled(null, BackgroundTaskCancellationReason.Abort);
            //BackgroundMediaPlayer.Shutdown();
        }

        private void SendMessage(string message, string content = "")
        {
            var value = new ValueSet();
            value.Add(message, content);
            BackgroundMediaPlayer.SendMessageToForeground(value);
        }

        private void UpdateAppTile()
        {

        }

        #region Timer

        ThreadPoolTimer timer = null;
        bool isTimerSet = false;

        private void SetTimer()
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
            if (delay > TimeSpan.Zero)
            {
                if (isTimerSet)
                {
                    TimerCancel();
                }
                timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(TimerCallback), delay);
                isTimerSet = true;
            }
        }

        private void TimerCallback(ThreadPoolTimer timer)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            TimerCancel();
            ShutdownPlayer();
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
    }


    
}
