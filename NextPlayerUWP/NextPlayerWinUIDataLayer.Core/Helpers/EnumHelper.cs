using System;

namespace NextPlayerWinUIDataLayer.Helpers
{
    public static class EnumHelper
    {
        public static T Parse<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}
