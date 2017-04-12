using System.Collections.Generic;

namespace NextPlayerUWP.Controls
{
    public interface IGetSelectedItems
    {
        List<T> GetSelectedItems<T>();
    }
}
