﻿using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Services
{
    public sealed class NowPlayingManager
    {
        private MediaPlayer mediaPlayer;

        TimeSpan startPosition;
        private DateTime songsStart;
        private TimeSpan songPlayed;

        private bool paused;
        private bool isFirst;

        private Playlist playlist;

        public NowPlayingManager()
        {
            playlist = new Playlist();

            startPosition = TimeSpan.Zero;
            songPlayed = TimeSpan.Zero;
            songsStart = DateTime.MinValue;

            paused = false;
            isFirst = true;

            mediaPlayer = BackgroundMediaPlayer.Current;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            //mediaPlayer.CurrentStateChanged += mediaPlayer_CurrentStateChanged;
            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
        }

        private async Task LoadFile(string path)
        {
            try
            {
                NowPlayingSong song = playlist.GetCurrentSong();
                //if (song == null)
                //{
                //    //exception
                //    throw new Exception("end of playlist");
                //}
                StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                mediaPlayer.AutoPlay = false;
                mediaPlayer.SetFileSource(file);
            }
            catch (Exception e)
            {
                //open default empty song
                if (!paused)
                {
                    Pause();
                }
                //ValueSet message = new ValueSet();
                //message.Add(AppConstants.ShutdownBGPlayer, "");
                //BackgroundMediaPlayer.SendMessageToBackground(message);

                //if (currentSongIndex >= 0 && currentSongIndex < songList.Count)
                //{
                //    if (!paused)
                //    {
                //        Pause();
                //    }
                //    Diagnostics.Logger.SaveBG("NPManager LoadSong() index OK" + "\n" + e.Message);
                //    Diagnostics.Logger.SaveToFileBG();
                //}
                //else
                //{
                //    Diagnostics.Logger.SaveBG("NPManager LoadSong() index not OK" + "\n" + e.Message);
                //    Diagnostics.Logger.SaveToFileBG();

                //    ValueSet message = new ValueSet();
                //    message.Add(AppConstants.ShutdownBGPlayer, "");
                //    BackgroundMediaPlayer.SendMessageToBackground(message);
                //}
            }

        }

        public async Task PlaySong(int index)
        {
            if (!isFirst)
            {
                StopSongEvent();
            }
            else
            {
                isFirst = false;
            }
            playlist.ChangeSong(index);
            paused = false;
            await LoadFile(playlist.GetCurrentSong().Path);
        }

        public async Task ResumePlayback()
        {
            if (mediaPlayer.CurrentState == MediaPlayerState.Playing || mediaPlayer.CurrentState == MediaPlayerState.Paused)
            {
                SendPosition();
                return;
            }
            paused = false;
            isFirst = false;
            object position = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Position);
            if (position != null)
            {
                startPosition = TimeSpan.Parse(position.ToString());
            }
            await LoadFile(playlist.GetCurrentSong().Path);
        }

        public void Play()
        {
            mediaPlayer.Play();
            paused = false;
            songsStart = DateTime.Now;
        }

        public void Pause()
        {
            mediaPlayer.Pause();
            paused = true;
            songPlayed = DateTime.Now - songsStart;
        }

        public async Task Next(bool userchoice = true)
        {
            StopSongEvent();
            if (playlist.NextSong(userchoice) == null)
            {
                paused = true;
                return;
            }
            await LoadFile(playlist.GetCurrentSong().Path);
            if (!userchoice)
            {
                ValueSet message = new ValueSet();
                message.Add(AppConstants.UpdateUVC, null);
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
            SendIndex();
        }

        public async Task Previous()
        {
            if (mediaPlayer.Position > TimeSpan.FromSeconds(5))
            {
                mediaPlayer.Position = TimeSpan.Zero;
            }
            else
            {
                StopSongEvent();
                playlist.PreviousSong();
                await LoadFile(playlist.GetCurrentSong().Path);
                SendIndex();
            }
        }

        public void LoadPlaylist()
        {
            playlist.LoadSongsFromDB();
        }

        public void UpdateSong(int songId, string title, string artist)
        {
            playlist.UpdateSong(songId, title, artist);
        }

        private void StopSongEvent()
        {
            //UpdateSongStatistics();
            //if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLogin).ToString() != "" && BackgroundMediaPlayer.Current.NaturalDuration != TimeSpan.Zero)
            //{
            //    System.Diagnostics.Debug.WriteLine("scrobble");
            //    ScrobbleTrack();
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine("no scrobble");
            //}
        }

        private void SendIndex()
        {
            ValueSet message = new ValueSet();
            message.Add(AppConstants.SongIndex, playlist.CurrentIndex.ToString());
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        private void SendPosition()
        {
            ValueSet message = new ValueSet();
            message.Add(AppConstants.Position, mediaPlayer.Position.ToString());
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        public void CompleteUpdate()
        {
            //SendIndex();
            ValueSet message = new ValueSet();
            message.Add(AppConstants.MediaOpened, BackgroundMediaPlayer.Current.NaturalDuration);
            BackgroundMediaPlayer.SendMessageToForeground(message);
            SendPosition();
        }

        private void ScrobbleTrack()
        {
            if (!paused)
            {
                try
                {
                    songPlayed += DateTime.Now - songsStart;
                }
                catch (Exception ex)
                {
                    Diagnostics.Logger.SaveBG("Scrobble !paused" + Environment.NewLine + ex.Data + Environment.NewLine + ex.Message);
                    Diagnostics.Logger.SaveToFileBG();
                }
            }
            if (songPlayed.TotalSeconds >= BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds * 0.5 || songPlayed.TotalSeconds >= 4 * 60)
            {
                int seconds = 0;
                try
                {
                    DateTime start = DateTime.UtcNow - songPlayed;
                    seconds = (int)start.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                }
                catch (Exception ex)
                {
                    Diagnostics.Logger.SaveBG("Scrobble paused" + Environment.NewLine + ex.Data + Environment.NewLine + ex.Message);
                    Diagnostics.Logger.SaveToFileBG();
                    return;
                }
                string artist = playlist.GetCurrentSong().Artist;
                string track = playlist.GetCurrentSong().Title;
                string timestamp = seconds.ToString();
                //TrackScrobble scrobble = new TrackScrobble()
                //{
                //    Artist = artist,
                //    Track = track,
                //    Timestamp = timestamp
                //};
                ////System.Diagnostics.Debug.WriteLine("scrobble " + artist + " " + track + " " + songPlayed);
                ////SendScrobble(scrobble);
                //LastFmManager.Current.CacheTrackScrobble(scrobble);
            }
        }

        //private async Task SendScrobble(TrackScrobble s)
        //{
        //    await Task.Run(() => LastFmManager.Current.CacheTrackScrobble(s));
        //}

        private void ScrobbleNowPlaying()
        {
            //if ((bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmSendNP))
            //{
            //    string artist = playlist.GetCurrentSong().Artist;
            //    string track = playlist.GetCurrentSong().Title;
            //    SendNowPlayingScrobble(artist, track);
            //}
        }

        private async Task SendNowPlayingScrobble(string artist, string track)
        {
            //await Task.Run(() => LastFmManager.Current.TrackUpdateNowPlaying(artist, track));
        }

        public void UpdateSongStatistics()
        {
            if (playlist.GetCurrentSong().SongId > 0 && BackgroundMediaPlayer.Current.Position.TotalSeconds >= 5.0)
            {
                //DatabaseManager.UpdateSongStatistics(playlist.GetCurrentSong().SongId);
            }
        }

        void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            // wait for media to be ready
            ValueSet message = new ValueSet();
            message.Add(AppConstants.MediaOpened, "");
            BackgroundMediaPlayer.SendMessageToForeground(message);
            songPlayed = TimeSpan.Zero;
            if (!paused)
            {
                sender.Play();
                songsStart = DateTime.Now;
                ScrobbleNowPlaying();
                if (!startPosition.Equals(TimeSpan.Zero))
                {
                    sender.Position = startPosition;
                    startPosition = TimeSpan.Zero;
                }
            }
            else
            {
                songsStart = DateTime.MinValue;
            }
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            Next(false);
        }

        private void mediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            //Debug.WriteLine("Failed with error code " + args.ExtendedErrorCode.ToString());
        }

        public void RemoveHandlers()
        {
            mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            //mediaPlayer.CurrentStateChanged -= mediaPlayer_CurrentStateChanged;
            mediaPlayer.MediaFailed -= mediaPlayer_MediaFailed;
            mediaPlayer = null;
        }

        public void ChangeRepeat()
        {
            playlist.ChangeRepeat();
        }

        public void ChangeShuffle()
        {
            playlist.ChangeShuffle();
        }

        public string GetArtist()
        {
            return playlist.GetCurrentSong().Artist;
        }

        public string GetTitle()
        {
            return playlist.GetCurrentSong().Title;
        }

        public void ChangeRate(int percent)
        {
            double rate = percent / 100.0;
            mediaPlayer.PlaybackRate = rate;
        }
    }


    class Playlist
    {
        private List<NowPlayingSong> playlist;
        private int currentIndex;
        private int previousIndex; // used in shuffle mode
        public int CurrentIndex { get { return currentIndex; } }
        public int SongsCount { get { return playlist.Count; } }
        private bool shuffle;
        private Queue<int> lastPlayed;
        private int maxQueueSize;
        private RepeatEnum repeat;

        private bool isSongRepeated;
        private bool isPlaylistRepeated;

        public Playlist()
        {
            lastPlayed = new Queue<int>();
            LoadSongsFromDB();
            previousIndex = -1;
            shuffle = Shuffle.CurrentState();
            repeat = Repeat.CurrentState();
            isPlaylistRepeated = false;
            isSongRepeated = false;
        }

        public Playlist(int index, bool shuffle, RepeatEnum repeat)
        {
            lastPlayed = new Queue<int>();
            LoadSongsFromDB();
            currentIndex = index;
            previousIndex = -1;
            this.shuffle = shuffle;
            this.repeat = repeat;
            isPlaylistRepeated = false;
            isSongRepeated = false;
        }

        public bool IsFirst()
        {
            return currentIndex == 0;
        }

        public bool IsLast()
        {
            return currentIndex == playlist.Count - 1;
        }

        //zwraca null, jesli nie ma nastepnego utworu do zagrania(np. jest koniec playlisty)
        public NowPlayingSong NextSong(bool userChoice)
        {
            NowPlayingSong song;
            bool stop = false;
            previousIndex = currentIndex;

            if (repeat == RepeatEnum.NoRepeat)
            {
                if (shuffle)
                {
                    currentIndex = GetRandomIndex();
                }
                else
                {
                    if (IsLast())
                    {
                        if (userChoice)
                        {
                            currentIndex = 0;
                        }
                        else
                        {
                            stop = true;
                        }
                    }
                    else
                    {
                        currentIndex++;
                    }
                }
            }
            if (repeat == RepeatEnum.RepeatOnce)
            {
                if (isSongRepeated)
                {
                    isSongRepeated = false;
                    if (shuffle)
                    {
                        currentIndex = GetRandomIndex();
                    }
                    else
                    {
                        if (IsLast())
                        {
                            if (userChoice)
                            {
                                currentIndex = 0;
                            }
                            else
                            {
                                stop = true;
                            }
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                }
                else
                {
                    if (userChoice)
                    {
                        if (shuffle)
                        {
                            currentIndex = GetRandomIndex();
                        }
                        else
                        {
                            if (IsLast())
                            {
                                if (userChoice)
                                {
                                    currentIndex = 0;
                                }
                                else
                                {
                                    stop = true;
                                }
                            }
                            else
                            {
                                currentIndex++;
                            }
                        }
                    }
                    else
                    {
                        isSongRepeated = true;
                    }
                }
            }
            else if (repeat == RepeatEnum.RepeatPlaylist)
            {
                if (shuffle)
                {
                    currentIndex = GetRandomIndex();
                }
                else
                {
                    if (isPlaylistRepeated)
                    {
                        if (IsLast())
                        {
                            if (userChoice)
                            {
                                currentIndex = 0;
                            }
                            else
                            {
                                currentIndex = 0;
                                //stop = true;
                            }
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                    else
                    {
                        if (IsLast())
                        {
                            currentIndex = 0;
                            isPlaylistRepeated = true;
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                }
            }
            if (stop)
            {
                return null;
            }

            ApplicationSettingsHelper.SaveSongIndex(currentIndex);
            song = GetCurrentSong();
            return song;
        }

        public NowPlayingSong PreviousSong()
        {
            NowPlayingSong song;

            isSongRepeated = false;
            if (shuffle)
            {
                if (previousIndex == -1)
                {
                    currentIndex = GetRandomIndex();
                }
                else
                {
                    currentIndex = previousIndex;
                    previousIndex = -1;
                }
            }
            else
            {
                if (IsFirst())
                {
                    currentIndex = playlist.Count - 1;
                }
                else
                {
                    currentIndex--;
                }
            }
            ApplicationSettingsHelper.SaveSongIndex(currentIndex);
            song = GetCurrentSong();
            return song;
        }

        public void ChangeShuffle()
        {
            shuffle = !shuffle;
            if (!shuffle)
            {
                lastPlayed.Clear();
            }
            else
            {
                lastPlayed.Enqueue(currentIndex);
            }
        }

        public void ChangeRepeat()
        {
            repeat = Repeat.CurrentState();
            isPlaylistRepeated = false;
            isSongRepeated = false;
        }

        public void LoadSongsFromDB()
        {
            playlist = DatabaseManager.Current.GetNowPlayingSongs();
            currentIndex = ApplicationSettingsHelper.ReadSongIndex();
            if (currentIndex >= playlist.Count)
            {
                currentIndex = playlist.Count - 1;
            }
            //if (currentIndex < 0)
            //{
            //    currentIndex = 0;
            //}
            lastPlayed.Clear();
            if (playlist.Count < 20)
            {
                maxQueueSize = playlist.Count;
            }
            else if (playlist.Count > 60)
            {
                maxQueueSize = 15;
            }
            else
            {
                maxQueueSize = 10 + (playlist.Count / 4);
            }
        }

        public void UpdateSong(int songId, string title, string artist)
        {
            foreach (var song in playlist)
            {
                if (song.SongId == songId)
                {
                    song.Title = title;
                    song.Artist = artist;
                }
            }
        }

        private int GetRandomIndex()
        {
            if (playlist.Count == 1)
            {
                return 0;
            }
            Random rnd = new Random();
            int r = rnd.Next(playlist.Count);
            while (r == currentIndex || (playlist.Count > 5 && lastPlayed.Contains(r)))
            {
                r = rnd.Next(playlist.Count);
            }
            if (maxQueueSize == playlist.Count && lastPlayed.Count == maxQueueSize - 1)
            {
                lastPlayed.Clear();
            }
            if (lastPlayed.Count == maxQueueSize)
            {
                lastPlayed.Dequeue();
            }
            lastPlayed.Enqueue(r);
            return r;
        }

        public NowPlayingSong GetCurrentSong()
        {
            if (currentIndex < playlist.Count && currentIndex >= 0)
            {
                return playlist.ElementAt(currentIndex);
            }
            else return new NowPlayingSong() { Artist = "-", Title = "-", Path = "", Position = -1, SongId = -1 };
        }

        public NowPlayingSong ChangeSong(int index)
        {
            previousIndex = currentIndex;
            currentIndex = index;
            isSongRepeated = false;
            ApplicationSettingsHelper.SaveSongIndex(currentIndex);
            return GetCurrentSong();
        }
    }
}
