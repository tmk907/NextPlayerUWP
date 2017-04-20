using System;

namespace NextPlayerUWPDataLayer.Helpers
{
    public static class ExtensionMethods
    {
        public static string GetExtension(this string path)
        {
            int lastIndex = path.LastIndexOf('.');
            if (lastIndex == -1) return String.Empty;
            else return path.Substring(lastIndex);
        }
    }
}
