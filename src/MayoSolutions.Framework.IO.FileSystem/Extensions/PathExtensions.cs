using System.IO;

namespace MayoSolutions.Framework.IO.Extensions
{
    public static class PathExtensions
    {
        public static string TrimPath(this string path)
        {
            return path?.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );
        }
    }
}