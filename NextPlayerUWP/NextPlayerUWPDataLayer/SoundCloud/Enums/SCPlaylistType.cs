using System;

namespace NextPlayerUWPDataLayer.SoundCloud.Enums
{
    public enum SCPlaylistType
    {
        [String("ep single")]
        EpSingle,
        [String("album")]
        Album,
        [String("compilation")]
        Compilation,
        [String("project files")]
        ProjectFiles,
        [String("archive")]
        Archive,
        [String("showcase")]
        Showcase,
        [String("demo")]
        Demo,
        [String("sample pack")]
        SamplePack,
        [String("other")]
        Other
    }
}
