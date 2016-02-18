namespace NextPlayerUWPDataLayer.Model
{
    public class NowPlayingListItem : MusicItem
    {
        public override string GetParameter()
        {
            return MusicItemTypes.nowplayinglist + separator + "np";
        }
    }
}
