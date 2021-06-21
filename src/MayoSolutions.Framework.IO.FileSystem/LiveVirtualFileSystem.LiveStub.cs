using System;
using System.IO;
using System.Text;

namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem
    {
        internal partial class LiveStub : IDrive, IDirectory, IFile
        {
            private readonly LiveVirtualFileSystem _fileSystem;
            private readonly IDrive _actualDrive;
            private readonly IDirectory _actualDirectory;
            private readonly IFile _actualFile;

            public LiveStub(LiveVirtualFileSystem fileSystem)
            {
                _fileSystem = fileSystem;
                _actualDrive = fileSystem.Drive;
                _actualDirectory = fileSystem.Directory;
                _actualFile = fileSystem.File;
            }

            private void InvalidateFileSystem()
            {
                _fileSystem.Invalidate(this);
            }

            string[] IDrive.GetDrives()
            {
                return _actualDrive.GetDrives();
            }

            VolumeInfo IDrive.GetVolumeInfo(string drive)
            {
                return _actualDrive.GetVolumeInfo(drive);
            }

            void IDirectory.CreateDirectory(string path)
            {
                _actualDirectory.CreateDirectory(path);
                InvalidateFileSystem();
            }

            string IFile.ReadAllText(string path)
            {
                return _actualFile.ReadAllText(path);
            }

            string IFile.ReadAllText(string path, Encoding encoding)
            {
                return _actualFile.ReadAllText(path, encoding);
            }

            bool IFile.Exists(string path)
            {
                return _actualFile.Exists(path);
            }

            void IFile.WriteAllText(string path, string contents)
            {
                _actualFile.WriteAllText(path, contents);
                InvalidateFileSystem();
            }

            void IFile.WriteAllText(string path, string contents, Encoding encoding)
            {
                _actualFile.WriteAllText(path, contents, encoding);
                InvalidateFileSystem();
            }

            void IFile.Delete(string path)
            {
                _actualFile.Delete(path);
                InvalidateFileSystem();
            }

            void IFile.Rename(string srcFileName, string destFileName)
            {
                _actualFile.Rename(srcFileName, destFileName);
                InvalidateFileSystem();
            }

            void IFile.Move(string srcFileName, string destFileName)
            {
                _actualFile.Move(srcFileName, destFileName);
                InvalidateFileSystem();
            }

            Stream IFile.Open(string path, FileMode fileMode)
            {
                return _actualFile.Open(path, fileMode);
            }

            Stream IFile.Open(string path, FileMode fileMode, FileAccess fileAccess)
            {
                return _actualFile.Open(path, fileMode, fileAccess);
            }

            Stream IFile.Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
            {
                return _actualFile.Open(path, fileMode, fileAccess, fileShare);
            }

            void IFile.SetCreationTime(string fileName, DateTime creationTime)
            {
                _actualFile.SetCreationTime(fileName, creationTime);
                InvalidateFileSystem();
            }

            void IFile.SetCreationTimeUtc(string fileName, DateTime creationTimeUtc)
            {
                _actualFile.SetCreationTimeUtc(fileName, creationTimeUtc);
                InvalidateFileSystem();
            }

            DateTime IFile.GetCreationTime(string fileName)
            {
                return _actualFile.GetCreationTime(fileName);
            }

            DateTime IFile.GetCreationTimeUtc(string fileName)
            {
                return _actualFile.GetCreationTimeUtc(fileName);
            }

            void IFile.SetLastWriteTime(string fileName, DateTime lastWriteTime)
            {
                _actualFile.SetLastWriteTime(fileName, lastWriteTime);
                InvalidateFileSystem();
            }

            void IFile.SetLastWriteTimeUtc(string fileName, DateTime lastWriteTimeUtc)
            {
                _actualFile.SetLastWriteTimeUtc(fileName, lastWriteTimeUtc);
                InvalidateFileSystem();
            }

            DateTime IFile.GetLastWriteTime(string fileName)
            {
                return _actualFile.GetLastWriteTime(fileName);
            }

            DateTime IFile.GetLastWriteTimeUtc(string fileName)
            {
                return _actualFile.GetLastWriteTimeUtc(fileName);
            }

            bool IDirectory.Exists(string path)
            {
                return _actualDirectory.Exists(path);
            }

            void IDirectory.Delete(string path)
            {
                _actualDirectory.Delete(path);
            }

            void IDirectory.Delete(string path, bool recursive)
            {
                _actualDirectory.Delete(path, recursive);
            }

            string[] IDirectory.GetDirectories(string path)
            {
                return _actualDirectory.GetDirectories(path);
            }

            string[] IDirectory.GetFiles(string path)
            {
                return _actualDirectory.GetFiles(path);
            }

            string[] IDirectory.GetFiles(string path, string searchPattern)
            {
                return _actualDirectory.GetFiles(path, searchPattern);
            }

            string[] IDirectory.GetFiles(string path, string searchPattern, SearchOption searchOption)
            {
                return _actualDirectory.GetFiles(path, searchPattern, searchOption);
            }

            void IDirectory.SetCreationTime(string path, DateTime creationTime)
            {
                _actualDirectory.SetCreationTime(path, creationTime);
                InvalidateFileSystem();
            }

            void IDirectory.SetCreationTimeUtc(string path, DateTime creationTimeUtc)
            {
                _actualDirectory.SetCreationTimeUtc(path, creationTimeUtc);
                InvalidateFileSystem();
            }

            DateTime IDirectory.GetCreationTime(string path)
            {
                return _actualDirectory.GetCreationTime(path);
            }

            DateTime IDirectory.GetCreationTimeUtc(string path)
            {
                return _actualDirectory.GetCreationTimeUtc(path);
            }

            void IDirectory.SetLastWriteTime(string path, DateTime lastWriteTime)
            {
                _actualDirectory.SetLastWriteTime(path, lastWriteTime);
                InvalidateFileSystem();
            }

            void IDirectory.SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
            {
                _actualDirectory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
                InvalidateFileSystem();
            }

            DateTime IDirectory.GetLastWriteTime(string path)
            {
                return _actualDirectory.GetLastWriteTime(path);
            }

            DateTime IDirectory.GetLastWriteTimeUtc(string path)
            {
                return _actualDirectory.GetLastWriteTimeUtc(path);
            }

            void IDirectory.Rename(string srcDirectoryName, string destDirectoryName)
            {
                _actualDirectory.Rename(srcDirectoryName, destDirectoryName);
                InvalidateFileSystem();
            }

            void IDirectory.Move(string srcDirectoryName, string destDirectoryName)
            {
                _actualDirectory.Move(srcDirectoryName, destDirectoryName);
                InvalidateFileSystem();
            }
        }
    }
}