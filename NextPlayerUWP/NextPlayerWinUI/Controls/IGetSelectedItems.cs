using System.Collections.Generic;

namespace NextPlayerWinUI.Controls
{
    public interface IGetSelectedItems
    {
        List<T> GetSelectedItems<T>();
    }
}
