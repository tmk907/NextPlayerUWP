using Microsoft.Toolkit.Uwp.Notifications;

namespace NextPlayerWinUI.Common.Tiles
{
    public interface ITileContentFactory
    {
        TileBindingContentAdaptive GetSmallBindingContent();
        TileBindingContentAdaptive GetMediumBindingContent();
        TileBindingContentAdaptive GetWideBindingContent();
        TileBindingContentAdaptive GetLargeBindingContent();
    }
}
