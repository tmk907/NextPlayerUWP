using NextPlayerUWP.Playback;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Common.History
{
    public class HistTrack
    {
        public int SongId { get; set; }
        public DateTime DatePlayed { get; set; }
        public TimeSpan PlaybackDuration { get; set; }
    }

    public enum TrackPlaybackStatus
    {
        Start,
        Resume,
        Pause,
        Complete
    }

    public class ListeningHistory
    {
        private ListeningHistoryRepository repo;

        private int songId;
        private DateTime startedtAt;
        private TrackPlaybackStatus prevStatus;
        private DateTime prevEventTime;
        private TimeSpan playbackTime = TimeSpan.Zero;

        public ListeningHistory()
        {
            repo = new ListeningHistoryRepository();
        }

        public void StartListening()
        {
            PlayingTrackEvents.MediaPlayerTrackStarted += MediaPlayerTrackStarted;
            PlayingTrackEvents.MediaPlayerTrackPaused += MediaPlayerTrackPaused;
            PlayingTrackEvents.MediaPlayerTrackResumed += MediaPlayerTrackResumed;
            PlayingTrackEvents.MediaPlayerTrackCompleted += MediaPlayerTrackCompleted;
        }

        public void StopListening()
        {
            PlayingTrackEvents.MediaPlayerTrackStarted -= MediaPlayerTrackStarted;
            PlayingTrackEvents.MediaPlayerTrackPaused -= MediaPlayerTrackPaused;
            PlayingTrackEvents.MediaPlayerTrackResumed -= MediaPlayerTrackResumed;
            PlayingTrackEvents.MediaPlayerTrackCompleted -= MediaPlayerTrackCompleted;
        }

        private void MediaPlayerTrackCompleted(int songId)
        {
            if (prevStatus == TrackPlaybackStatus.Resume)
            {
                playbackTime = DateTime.Now - prevEventTime;
            }
            else if (prevStatus == TrackPlaybackStatus.Pause)
            {

            }
            else
            {
                throw new Exception();
            }
            prevEventTime = DateTime.Now;

            if (playbackTime > TimeSpan.FromSeconds(5))
            {
                repo.Add(new HistTrack()
                {
                    DatePlayed = startedtAt,
                    PlaybackDuration = playbackTime,
                    SongId = songId,
                });
            }
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

        private void MediaPlayerTrackStarted(int songId, MediaPlaybackState2 state)
        {
            prevStatus = state == MediaPlaybackState2.Playing ? TrackPlaybackStatus.Resume : TrackPlaybackStatus.Pause;
            prevEventTime = DateTime.Now;
            playbackTime = TimeSpan.Zero;
            startedtAt = DateTime.Now;
            this.songId = songId;
        }
    }

    public class ListeningHistoryRepository
    {
        private List<HistTrack> history = new List<HistTrack>();

        public async Task Add(HistTrack ht)
        {
            System.Diagnostics.Debug.WriteLine("History {0} {1} {2}", ht.SongId, ht.PlaybackDuration.ToString(), ht.DatePlayed.ToString());
            history.Add(ht);
            await Task.CompletedTask;
        }
    }
}
