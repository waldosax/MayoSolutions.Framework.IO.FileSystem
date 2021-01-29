using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MayoSolutions.Framework.IO.Extensions;

namespace MayoSolutions.Framework.IO
{
    public static class FileSystemUtility
    {
        public static string[] GetAllFoldersBetween(string a, string b)
        {
            List<string> list = new List<string>();

            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return list.ToArray();

            var slash = Path.DirectorySeparatorChar;
            a = a.TrimEnd(slash) + slash;
            b = b.TrimEnd(slash) + slash;

            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                return list.ToArray();

            string current = new string(b.ToCharArray());
            while (current.StartsWith(a, StringComparison.OrdinalIgnoreCase))
            {
                string parent = Path.GetDirectoryName(Path.GetDirectoryName(current)).TrimEnd(slash) + slash;
                if (string.Equals(parent, a, StringComparison.OrdinalIgnoreCase)) break;
                list.Add(parent);
                current = parent;
            }

            return list.ToArray();
        }

        public static string GetGreatestCommonDirectory(string[] directories)
        {
            string gcd = string.Empty;

            var allPathParts = directories
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(ParsePath)
                .OrderBy(arr => arr.Length)
                .ToArray();

            var baseline = allPathParts.First();
            int length = baseline.Length;

            for (int i = 1; i < length; i++)
            {
                if (!allPathParts.All(arr =>
                    string.Equals(baseline[i], arr[i], StringComparison.OrdinalIgnoreCase))) break;

                if (string.IsNullOrEmpty(gcd)) gcd = baseline[0];
                gcd = Path.Combine(gcd, baseline[i]);
            }

            return gcd;
        }

        public static string[] ParsePath(string path)
        {
            return ParsePath(path, Path.DirectorySeparatorChar);
        }

        public static string[] ParsePath(string path, char directorySeparatorChar)
        {
            // *********************************
            //      How first nodes are treated
            // *********************************
            //
            // Windows
            //  Volume
            //      C:\
            //  Mapped Drive
            //      \\hostname\
            //
            // *NIX, MacOS
            //  Root
            //      /
            //  Mapped Drive
            //      //hostname/

            List<string> pathNodes = new List<string>();
            string tmp = path.TrimPath();

            string uncIndicator = new string(directorySeparatorChar, 2);
            bool isUncPath = tmp.StartsWith(uncIndicator);

            int startIndex = 0;
            int indexOfDirectorySeparator = tmp.IndexOf(directorySeparatorChar, isUncPath ? 2 : 0);
            if (!isUncPath && directorySeparatorChar == '/' && indexOfDirectorySeparator == 0)
            {
                pathNodes.Add(directorySeparatorChar.ToString());
                startIndex = 1;
                indexOfDirectorySeparator = tmp.IndexOf(directorySeparatorChar, 1); ;
            }

            while (indexOfDirectorySeparator > startIndex)
            {
                var node = tmp.Substring(startIndex, indexOfDirectorySeparator - startIndex);
                if (node.Length > 0) pathNodes.Add(node);
                startIndex += node.Length + 1;
                indexOfDirectorySeparator = tmp.IndexOf(directorySeparatorChar, startIndex);
            }

            if (startIndex < tmp.Length)
            {
                var node = tmp.Substring(startIndex);
                pathNodes.Add(node);
            }

            return pathNodes.ToArray();
        }


        public static string MakeRelative(string path1, string path2)
        {
            path1 = path1.TrimEnd(Path.DirectorySeparatorChar);
            path2 = path2.TrimEnd(Path.DirectorySeparatorChar);

            string x = path1, y = path2;
            if (path1.Length > path2.Length)
            {
                y = path1;
                x = path2;
            }

            if (!y.StartsWith(x, StringComparison.OrdinalIgnoreCase))
                return y;

            return y.Substring(x.Length).TrimStart(Path.DirectorySeparatorChar);
        }







        public static string SanitizePath(string path)
        {
            return SanitizePath(path, Path.DirectorySeparatorChar);
        }

        internal static string SanitizePath(string path, char directorySeparatorChar)
        {
            if (directorySeparatorChar == '/')
            {
                if (path.IndexOf(':') >= 0) path = path.Replace(":", "");
                path = path.Replace('\\', '/');
                if (path.IndexOf('/') != 0) path = "/" + path;
            }
            else if (directorySeparatorChar == '\\')
            {
                path = path.Replace('/', '\\');
            }
            return path;
        }

    }
}
