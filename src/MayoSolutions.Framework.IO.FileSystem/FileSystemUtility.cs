using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            List<string> pathNodes = new List<string>();
            string tmp = new string(path.ToCharArray());
            int indexOfDirectorySeparator = tmp.LastIndexOf(Path.DirectorySeparatorChar);
            while (indexOfDirectorySeparator > 1)
            {
                string nodeName = tmp.Substring(indexOfDirectorySeparator + 1);
                if (nodeName.Length > 0) pathNodes.Insert(0, nodeName);
                tmp = tmp.Substring(0, indexOfDirectorySeparator);
                indexOfDirectorySeparator = tmp.LastIndexOf(Path.DirectorySeparatorChar);
            }
            if (tmp.Length > 0) pathNodes.Insert(0, tmp);
            return pathNodes.ToArray();
        }

    }
}
