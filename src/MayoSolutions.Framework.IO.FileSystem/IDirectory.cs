using System;
using System.IO;

namespace MayoSolutions.Framework.IO
{
    public interface IDirectory
    {
        void CreateDirectory(string path);
        bool Exists(string path);
        void Delete(string path);
        void Delete(string path, bool recursive);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
        string[] GetFiles(string path, string searchPattern);
        string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

        void SetLastWriteTime(string path, DateTime lastWriteTime);
        void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);

        void Rename(string srcDirectoryName, string destDirectoryName);
        void Move(string srcDirectoryName, string destDirectoryName);
    }
}