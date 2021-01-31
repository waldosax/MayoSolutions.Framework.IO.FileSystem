using System;

namespace MayoSolutions.Framework.IO.Extensions
{
    public static class PathExtensions
    {
        public static bool IsWindowsPath(this string path)
        {
            if (path == null) throw new ArgumentNullException("path", "Path was not supplied.");
            return path.IndexOf(@"\\") == 0 || path.IndexOfAny(new[] { '\\', ':' }) >= 0;
        }
        public static bool IsLinuxPath(this string path)
        {
            if (path == null) throw new ArgumentNullException("path", "Path was not supplied.");
            return path.IndexOf(@"//") == 0 || path.IndexOfAny(new[] { '/' }) >= 0;
        }

        public static string TrimPath(this string path)
        {
            if (path == "/") return path;
            return path?.TrimEnd(
                '\\',
                '/'
            );
        }
    }
}