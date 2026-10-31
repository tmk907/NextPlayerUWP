using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace NextPlayerWinUI.Controls
{
    /// <summary>Drag handle for resizing a side panel; shows the horizontal resize cursor.</summary>
    public sealed partial class PaneResizer : Grid
    {
        public PaneResizer()
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
        }
    }
}
