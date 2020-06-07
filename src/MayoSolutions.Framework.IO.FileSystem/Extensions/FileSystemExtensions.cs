using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MayoSolutions.Framework.IO.Extensions
{
    public static class FileSystemExtensions
    {

        public static string[] GetDirectories(this IFileSystem fileSystem, string path)
        {
            string[] directories = new string[0];
            try
            {
                directories = fileSystem.Directory.GetDirectories(path);
            }
            catch (DirectoryNotFoundException) { }

            return directories;
        }

        public static string[] GetFilesRecursive(this IFileSystem fileSystem, string path)
        {
            List<string> files = new List<string>();
            return GetFilesRecursive(fileSystem, path, ref files);
        }

        public static string[] GetFilesRecursive(this IFileSystem fileSystem, string path, ref List<string> files)
        {
            if (files == null) files = new List<string>();

            files.AddRange(GetFiles(fileSystem, path));
            foreach (string directory in fileSystem.Directory.GetDirectories(path))
            {
                GetFilesRecursive(fileSystem, Path.Combine(path, directory), ref files);
            }

            return files.ToArray();
        }

        public static string[] GetFiles(this IFileSystem fileSystem, string path)
        {
            string[] files = new string[0];
            try
            {
                files = fileSystem.Directory.GetFiles(path);
            }
            catch (DirectoryNotFoundException) { }
            catch (FileNotFoundException) { }

            return files;
        }

        public static bool FolderIsEmpty(this IFileSystem fileSystem, string path)
        {
            string[] files = GetFiles(fileSystem, path);
            if (files.Any()) return false;

            string[] directories = GetDirectories(fileSystem, path);
            if (!directories.Any()) return true;

            return directories.All(d => FolderIsEmpty(fileSystem, Path.Combine(path, d)));
        }
    }
}

