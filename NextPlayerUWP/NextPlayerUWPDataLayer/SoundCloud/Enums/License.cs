using System;

namespace NextPlayerUWPDataLayer.SoundCloud.Enums
{
    public enum TrackLicense
    {
        [String("no-rights-reserved")]
        NoRightsReserved,
        [String("all -rights-reserved")]
        AllRightsReserved,
        [String("cc-by")]
        CCBY,
        [String("cc-by-nc")]
        CCBYNC,
        [String("cc-by-nd")]
        CCBYND,
        [String("cc-by-sa")]
        CCBYSA,
        [String("cc-by-nc-nd")]
        CCBYNCND,
        [String("cc-by-nc-sa")]
        CCBYNCSA
    }
}
