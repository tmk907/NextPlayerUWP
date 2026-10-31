namespace NextPlayerWinUIDataLayer.Model
{
    public class SdCardFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IncludeSubFolders { get; set; }

        /// <summary>False for folders of the Windows Music library, which only Windows can change.</summary>
        public bool IsRemovable { get; set; }
    }
}
