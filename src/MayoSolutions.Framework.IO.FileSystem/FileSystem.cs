﻿using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MayoSolutions.Framework.IO
{
    public class FileSystem : IFileSystem
    {
        private class DirectoryStub : IDirectory
        {
            public void CreateDirectory(string path) => new DirectoryInfo(path).Create();
            public bool Exists(string path) => new DirectoryInfo(path).Exists;
            public void Delete(string path) => new DirectoryInfo(path).Delete();
            public void Delete(string path, bool recursive) => new DirectoryInfo(path).Delete(recursive);
            public string[] GetDirectories(string path) => new DirectoryInfo(path).GetDirectories().Select(x => x.FullName).ToArray();
            public string[] GetFiles(string path) => new DirectoryInfo(path).GetFiles().Select(x => x.FullName).ToArray();
            public string[] GetFiles(string path, string searchPattern) => new DirectoryInfo(path).GetFiles(searchPattern).Select(x => x.FullName).ToArray();
            public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => new DirectoryInfo(path).GetFiles(searchPattern, searchOption).Select(x => x.FullName).ToArray();
            public void SetLastWriteTime(string path, DateTime lastWriteTime) => new DirectoryInfo(path).CreationTime = lastWriteTime;
            public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) => new DirectoryInfo(path).CreationTimeUtc = lastWriteTimeUtc;
            public void Rename(string srcDirectoryName, string destDirectoryName) => Move(srcDirectoryName, destDirectoryName);
            public void Move(string srcDirectoryName, string destDirectoryName) =>
                new DirectoryInfo(srcDirectoryName).MoveTo(destDirectoryName);
        }

        private class FileStub : IFile
        {
            public string ReadAllText(string path) => ReadAllText(path, Encoding.Default);
            public string ReadAllText(string path, Encoding encoding)
            {
                var fileInfo = new FileInfo(path);
                using (var fs = fileInfo.OpenRead())
                {
                    using (var sr = new StreamReader(fs, encoding))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }

            public bool Exists(string path) => new FileInfo(path).Exists;

            public void WriteAllText(string path, string contents) => WriteAllText(path, contents, Encoding.Default);
            public void WriteAllText(string path, string contents, Encoding encoding)
            {
                var fileInfo = new FileInfo(path);
                using (var fs = fileInfo.OpenWrite())
                {
                    using (var sr = new StreamWriter(fs, encoding))
                    {
                        sr.Write(contents);
                    }
                }
            }

            public void Delete(string path) => new FileInfo(path).Delete();

            public void Rename(string srcFileName, string destFileName)
            {
                Move(srcFileName, destFileName);
            }

            public void Move(string srcFileName, string destFileName)
            {
                var fileInfo = new FileInfo(srcFileName);
                fileInfo.MoveTo(destFileName);
            }

            public Stream Open(string path, FileMode fileMode)
            {
                return System.IO.File.Open(path, fileMode);
            }
            public Stream Open(string path, FileMode fileMode, FileAccess fileAccess)
            {
                return System.IO.File.Open(path, fileMode, fileAccess);
            }
            public Stream Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
            {
                return System.IO.File.Open(path, fileMode, fileAccess, fileShare);
            }
        }


        public IDirectory Directory { get; }
        public IFile File { get; }

        public FileSystem()
        {
            Directory = new DirectoryStub();
            File = new FileStub();
        }
    }
}
