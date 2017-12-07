using NextPlayerUWP.Playback;
using NextPlayerUWPDataLayer.Services.Repository;
using System;

namespace NextPlayerUWP.Common.History
{
    public class ListeningHistory
    {
        private IRepository<HistTrack> repo;

        private enum TrackPlaybackStatus
        {
            Start,
            Resume,
            Pause,
            Complete
        }

        private TimeSpan minPlaybackDuration;

        private int songId;
        private DateTime startedtAt;
        private TrackPlaybackStatus prevStatus;
        private DateTime prevEventTime;
        private TimeSpan playbackTime = TimeSpan.Zero;

        public ListeningHistory()
        {
            repo = new ListeningHistoryRepository();
            minPlaybackDuration = TimeSpan.FromSeconds(5);
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

            if (playbackTime > minPlaybackDuration)
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
}
