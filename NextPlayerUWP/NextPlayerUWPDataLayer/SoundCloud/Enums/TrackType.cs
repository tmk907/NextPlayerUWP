using System;

namespace NextPlayerUWPDataLayer.SoundCloud.Enums
{
    public enum TrackType
    {
        [String("original")]
        Original,
        [String("remix")]
        Remix,
        [String("live")]
        Live,
        [String("recording")]
        Recording,
        [String("spoken")]
        Spoken,
        [String("podcast")]
        Podcast,
        [String("demo")]
        Demo,
        [String("in progress")]
        InProgress,
        [String("stem")]
        Stem,
        [String("loop")]
        Loop,
        [String("sound effect")]
        SoundEffect,
        [String("sample")]
        Sample,
        [String("other")]
        Other

    }
}
