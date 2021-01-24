using System;
using System.IO;
using System.Text;

namespace MayoSolutions.Framework.IO
{
    public interface IFile
    {
        string ReadAllText(string path);
        string ReadAllText(string path, Encoding encoding);
        bool Exists(string path);
        void WriteAllText(string path, string contents);
        void WriteAllText(string path, string contents, Encoding encoding);
        void Delete(string path);
        void Rename(string srcFileName, string destFileName);
        void Move(string srcFileName, string destFileName);
        Stream Open(string path, FileMode fileMode);
        Stream Open(string path, FileMode fileMode, FileAccess fileAccess);
        Stream Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
  
        void SetCreationTime(string fileName, DateTime creationTime);
        void SetCreationTimeUtc(string fileName, DateTime creationTimeUtc);
        DateTime GetCreationTime(string fileName);
        DateTime GetCreationTimeUtc(string fileName);

        void SetLastWriteTime(string fileName, DateTime lastWriteTime);
        void SetLastWriteTimeUtc(string fileName, DateTime lastWriteTimeUtc);
        DateTime GetLastWriteTime(string fileName);
        DateTime GetLastWriteTimeUtc(string fileName);
    }
}