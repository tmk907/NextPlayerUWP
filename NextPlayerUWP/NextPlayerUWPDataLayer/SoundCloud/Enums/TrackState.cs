using System;

namespace NextPlayerUWPDataLayer.SoundCloud.Enums
{
    public enum TrackState
    {
        [String("processing")]
        Processing = 1,

        [String("failed")]
        Failed = 2,

        [String("finished")]
        Finished = 3
    }
}
