using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MayoSolutions.Framework.IO
{
    public class FileSystem : IFileSystem
    {
        private static readonly string[] CaseInsensitiveDriveFormats = { "NTFS", "CDFS", "FAT", "FAT32" };
        private class DriveStub : IDrive
        {
            public string[] GetDrives() => DriveInfo.GetDrives().Select(di => di.Name).ToArray();
            public VolumeInfo GetVolumeInfo(string drive)
            {
                if (drive[drive.Length - 1] == Path.VolumeSeparatorChar) drive += Path.DirectorySeparatorChar;

                StringBuilder volumeNameBuffer = new StringBuilder(261);
                StringBuilder fileSystemNameBuffer = new StringBuilder(261);

                var di = new DriveInfo(drive);
                var vi = new VolumeInfo
                {
                    RootPathName = drive,
                    IsReady = di.IsReady,
                    DriveType = di.DriveType,
                    MappedPathName = di.DriveType == DriveType.Network ? di.RootDirectory.FullName : null
                };

                if (di.IsReady)
                {
                    vi.VolumeLabel = di.VolumeLabel;
                    vi.AvailableFreeSpace = di.AvailableFreeSpace;
                    vi.TotalFreeSpace = di.TotalFreeSpace;
                    vi.TotalSize = di.TotalSize;
                    vi.DriveFormat = di.DriveFormat;
                    vi.IsCaseSensitive = CaseInsensitiveDriveFormats.Contains(di.DriveFormat, StringComparer.OrdinalIgnoreCase);
                }


                if (Win32Api.GetVolumeInformation(drive,
                    volumeNameBuffer, 261, out _,
                    out _, out var fileSystemFlags,
                    fileSystemNameBuffer, 261))
                {
                    vi.VolumeLabel = !string.IsNullOrWhiteSpace(vi.VolumeLabel)
                        ? vi.VolumeLabel
                        : volumeNameBuffer.ToString();
                    vi.DriveFormat = fileSystemNameBuffer.ToString();
                    vi.IsCaseSensitive = fileSystemFlags.HasFlag(Win32Api.FileSystemFeature.CaseSensitiveSearch);
                    vi.IsCompressed = fileSystemFlags.HasFlag(Win32Api.FileSystemFeature.VolumeIsCompressed);
                    vi.SupportsEncryption = fileSystemFlags.HasFlag(Win32Api.FileSystemFeature.SupportsEncryption);
                    vi.SupportsCompression = fileSystemFlags.HasFlag(Win32Api.FileSystemFeature.FileCompression);
                    vi.SupportsHardLinks = fileSystemFlags.HasFlag(Win32Api.FileSystemFeature.SupportsHardLinks);
                }

                return vi;
                //throw new IOException($"Volume information for {drive} not found.");
            }
        }

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
            public DateTime GetCreationTime(string path) => new DirectoryInfo(path).CreationTime;
            public DateTime GetCreationTimeUtc(string path) => new DirectoryInfo(path).CreationTimeUtc;
            public void SetCreationTime(string path, DateTime creationTime) => new DirectoryInfo(path).CreationTime = creationTime;
            public void SetCreationTimeUtc(string path, DateTime creationTimeUtc) => new DirectoryInfo(path).CreationTime = creationTimeUtc;
            public DateTime GetLastWriteTime(string path) => new DirectoryInfo(path).LastWriteTime;
            public DateTime GetLastWriteTimeUtc(string path) => new DirectoryInfo(path).LastWriteTimeUtc;
            public void SetLastWriteTime(string path, DateTime lastWriteTime) => new DirectoryInfo(path).LastWriteTime = lastWriteTime;
            public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) => new DirectoryInfo(path).LastWriteTime = lastWriteTimeUtc;
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


            public DateTime GetCreationTime(string fileName) => new FileInfo(fileName).CreationTime;
            public DateTime GetCreationTimeUtc(string fileName) => new FileInfo(fileName).CreationTimeUtc;
            public void SetCreationTime(string fileName, DateTime creationTime) => new FileInfo(fileName).CreationTime = creationTime;
            public void SetCreationTimeUtc(string fileName, DateTime creationTimeUtc) => new FileInfo(fileName).CreationTime = creationTimeUtc;
            public DateTime GetLastWriteTime(string fileName) => new FileInfo(fileName).LastWriteTime;
            public DateTime GetLastWriteTimeUtc(string fileName) => new FileInfo(fileName).LastWriteTimeUtc;
            public void SetLastWriteTime(string fileName, DateTime lastWriteTime) => new FileInfo(fileName).LastWriteTime = lastWriteTime;
            public void SetLastWriteTimeUtc(string fileName, DateTime lastWriteTimeUtc) => new FileInfo(fileName).LastWriteTime = lastWriteTimeUtc;

        }

        public OSPlatform Platform => OperatingSystem.Platform;
        public StringComparer PathComparer =>
            Platform == OSPlatform.Windows || Platform == OSPlatform.OSX
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;
        public char DirectorySeparatorChar => Path.DirectorySeparatorChar;

        public IDrive Drive { get; }
        public IDirectory Directory { get; }
        public IFile File { get; }

        public FileSystem()
        {
            Drive = new DriveStub();
            Directory = new DirectoryStub();
            File = new FileStub();
        }
    }
}
