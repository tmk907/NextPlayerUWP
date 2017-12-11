using NextPlayerUWP.Playback;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using NextPlayerUWPDataLayer.Services.Repository;
using System;

namespace NextPlayerUWP.Common.History
{
    public class ListeningHistory
    {
        private ListeningHistoryRepository repo;
        private LastFmCache lastFmCache;

        private enum TrackPlaybackStatus
        {
            Start,
            Resume,
            Pause,
            Complete
        }

        private TimeSpan minPlaybackDuration;

        private SongItem song;
        private DateTime startedtAt;
        private TrackPlaybackStatus prevStatus;
        private DateTime prevEventTime;
        private TimeSpan playbackTime = TimeSpan.Zero;

        public ListeningHistory()
        {
            repo = new ListeningHistoryRepository();
            lastFmCache = new LastFmCache();
            minPlaybackDuration = TimeSpan.FromSeconds(5);
            song = new SongItem();
        }

        public void StartListening()
        {
            PlayingTrackStateEvents.MediaPlayerTrackStarted += MediaPlayerTrackStarted;
            PlayingTrackStateEvents.MediaPlayerTrackPaused += MediaPlayerTrackPaused;
            PlayingTrackStateEvents.MediaPlayerTrackResumed += MediaPlayerTrackResumed;
            PlayingTrackStateEvents.MediaPlayerTrackCompleted += MediaPlayerTrackCompleted;
        }

        public void StopListening()
        {
            PlayingTrackStateEvents.MediaPlayerTrackStarted -= MediaPlayerTrackStarted;
            PlayingTrackStateEvents.MediaPlayerTrackPaused -= MediaPlayerTrackPaused;
            PlayingTrackStateEvents.MediaPlayerTrackResumed -= MediaPlayerTrackResumed;
            PlayingTrackStateEvents.MediaPlayerTrackCompleted -= MediaPlayerTrackCompleted;
        }

        private async void MediaPlayerTrackCompleted(SongItem songItem)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayerTrackCompleted");
            if (song.SongId != songItem.SongId)
            {
                throw new Exception();
            }
            TimeSpan playbackDuration = TimeSpan.FromTicks(playbackTime.Ticks);
            DateTime playedAt = new DateTime(startedtAt.Ticks);
            if (prevStatus == TrackPlaybackStatus.Resume)
            {
                playbackDuration += DateTime.Now - prevEventTime;
            }
            else if (prevStatus == TrackPlaybackStatus.Pause)
            {

            }
            else
            {
                throw new Exception();
            }
            prevEventTime = DateTime.Now;
            if (songItem.SongId == -1) return;
            System.Diagnostics.Debug.WriteLine("MediaPlayerTrackCompleted2");
            if (playbackDuration > minPlaybackDuration)
            {
                await repo.Add(new ListenedSong()
                {
                    DatePlayed = playedAt,
                    PlaybackDuration = playbackDuration,
                    SongId = songItem.SongId,
                }).ConfigureAwait(false);
            }
            //!
            if (song.SongId > 0 && song.SourceType != MusicSource.RadioJamendo && song.SourceType != MusicSource.Radio)
            {
                if (playbackDuration.TotalSeconds >= song.Duration.TotalSeconds * 0.5 || playbackDuration.TotalSeconds >= 4 * 60)
                {
                    await DatabaseManager.Current.UpdateSongStatistics(song.SongId);
                }
            }
            await lastFmCache.CacheTrackScrobble(song, playbackDuration, playedAt).ConfigureAwait(false);
        }

        private void MediaPlayerTrackResumed()
        {
            prevStatus = TrackPlaybackStatus.Resume;
            prevEventTime = DateTime.Now;
        }

        private void MediaPlayerTrackPaused()
        {
            playbackTime += DateTime.Now - prevEventTime;

            prevStatus = TrackPlaybackStatus.Pause;
            prevEventTime = DateTime.Now;
        }

        private void MediaPlayerTrackStarted(SongItem songItem, MediaPlaybackState2 state)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayerTrackStarted");
            prevStatus = state == MediaPlaybackState2.Playing ? TrackPlaybackStatus.Resume : TrackPlaybackStatus.Pause;
            prevEventTime = DateTime.Now;
            playbackTime = TimeSpan.Zero;
            startedtAt = DateTime.Now;
            song = songItem;
        }
    }
}
