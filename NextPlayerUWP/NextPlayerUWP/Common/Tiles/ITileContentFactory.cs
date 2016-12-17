using Microsoft.Toolkit.Uwp.Notifications;

namespace NextPlayerUWP.Common.Tiles
{
    public interface ITileContentFactory
    {
        TileBindingContentAdaptive GetSmallBindingContent();
        TileBindingContentAdaptive GetMediumBindingContent();
        TileBindingContentAdaptive GetWideBindingContent();
        TileBindingContentAdaptive GetLargeBindingContent();
    }
}
