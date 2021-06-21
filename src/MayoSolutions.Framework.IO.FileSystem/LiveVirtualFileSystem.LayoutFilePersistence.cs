using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MayoSolutions.Framework.IO.Extensions;
using Newtonsoft.Json;

namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem
    {
        internal class LayoutFilePersistence : IPersistence, IDisposable
        {
            protected readonly LiveVirtualFileSystem FileSystem;
            protected readonly LiveVirtualFileSystemOptions Options;
            public bool IsLoading { get; private set; }
            public bool IsSaving { get; private set; }

            private FileSystemWatcher _fileSystemWatcher;

            public LayoutFilePersistence(
                LiveVirtualFileSystem fileSystem,
                LiveVirtualFileSystemOptions options
            )
            {
                FileSystem = fileSystem;
                Options = options;
            }

            private FileStream OpenFile(string filePath, bool forWriting)
            {
                FileStream stream = null;
                bool wasFileLocked = true;
                while (wasFileLocked)
                {
                    try
                    {
                        wasFileLocked = false;
                        stream = new FileStream(filePath,
                            forWriting ? FileMode.Create : FileMode.Open,
                            forWriting ? FileAccess.Write : FileAccess.Read
                        );
                    }
                    catch (FileNotFoundException)
                    {
                        break;
                    }
                    catch (IOException e)
                    {
                        var errorCode = e.HResult & ((1 << 16) - 1);
                        wasFileLocked = errorCode == 32 || errorCode == 33;
                        if (wasFileLocked) Thread.Sleep(100);
                        if (!wasFileLocked) throw;
                    }
                }

                return stream;
            }

            public void Load()
            {
                if (IsSaving) return;
                IsLoading = true;

                string json;
                var fs = OpenFile(Options.FileSystemLayoutFilePath, false);
                if (fs != null)
                {
                    using (fs)
                    using (var reader = new StreamReader(fs))
                        json = reader.ReadToEnd();

                    var root = JsonConvert.DeserializeObject<RootPersistenceNode>(json);

                    OSPlatform platform = OSPlatform.Create(root.Platform);
                    FileSystem.CreateInternal(platform);

                    FileSystem.Volumes.Clear();
                    Load(root);
                }

                // Add watch
                AddWatch();

                IsLoading = false;
            }


            private void Load(RootPersistenceNode root)
            {
                if (root.Volumes != null)
                    foreach (var volume in root.Volumes)
                    {
                        Load(volume);
                    }
            }
            private void Load(VolumePersistenceNode volume)
            {
                VolumeInfo volumeInfo = volume.VolumeInfo ?? new VolumeInfo // TODO: Default VolumeInfo
                {
                    RootPathName = volume.Name,
                    DriveType = DriveType.Fixed,
                    DriveFormat = "NTFS",
                    IsReady = true
                };

                if (string.IsNullOrEmpty(volumeInfo.RootPathName))
                    volumeInfo.RootPathName = volume.Name;

                FileSystem.NodeNavigator.GetOrCreateVolume(FileSystem.Volumes, volumeInfo, true);
                Load("", volume);
            }
            private void Load(string parentPath, ContainerPersistenceNode container)
            {
                string containerPath = container.Name?.TrimStart('\\', '/' );
                if (!string.IsNullOrEmpty(parentPath.TrimPath()))
                    containerPath = parentPath.TrimPath() + FileSystem.DirectorySeparatorChar + containerPath;

                if (!string.IsNullOrEmpty(containerPath))
                    WithPath(containerPath, container.CreationTimeUtc?.ToLocalTime(), container.LastWriteTimeUtc?.ToLocalTime());
                if (container.Directories != null)
                    foreach (var childDirectory in container.Directories)
                    {
                        Load(containerPath, childDirectory);
                    }
                if (container.Files != null)
                    foreach (var file in container.Files)
                    {
                        Load(containerPath, file);
                    }
            }
            private void Load(string parentPath, FilePersistenceNode file)
            {
                string filePath = parentPath.TrimPath() + FileSystem.DirectorySeparatorChar + file.Name.TrimStart( '\\', '/' );
                FileSystem.WithFile(filePath, file.Contents, file.CreationTimeUtc?.ToLocalTime(), file.LastWriteTimeUtc?.ToLocalTime());
            }



            public void Save()
            {
                // Do the saving things
                IsSaving = true;
                _fileSystemWatcher.EnableRaisingEvents = false;

                // Convert layout into persistable objects
                var persistable = new RootPersistenceNode
                {
                    Platform = FileSystem.Platform.ToString(),
                    Volumes = FileSystem.Volumes.Select(Save).ToList(),
                };

                // Save JSON to file
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    CheckAdditionalContent = false,
                };

                var json = JsonConvert.SerializeObject(persistable, Formatting.Indented, settings);

                using (var fs = OpenFile(Options.FileSystemLayoutFilePath, true))
                using (var writer = new StreamWriter(fs) { AutoFlush = true })
                    writer.Write(json);

                IsSaving = false;
            }

            private VolumePersistenceNode Save(VolumeNode volume)
            {
                return new VolumePersistenceNode
                {
                    Name = volume.Name,
                    VolumeInfo = volume.VolumeInfo,
                    Directories = volume.Directories?.Any() == true ? volume.Directories.Select(Save).ToList() : null,
                    Files = volume.Files?.Any() == true ? volume.Files.Select(Save).ToList() : null
                };
            }
            private DirectoryPersistenceNode Save(DirectoryNode directory)
            {
                return new DirectoryPersistenceNode
                {
                    Name = directory.Name,
                    CreationTimeUtc = directory.CreationTimeUtc,
                    LastWriteTimeUtc = directory.LastWriteTimeUtc,
                    Directories = directory.Directories?.Any() == true ? directory.Directories.Select(Save).ToList() : null,
                    Files = directory.Files?.Any() == true ? directory.Files.Select(Save).ToList() : null
                };
            }
            private FilePersistenceNode Save(FileNode file)
            {
                return new FilePersistenceNode
                {
                    Name = file.Name,
                    CreationTimeUtc = file.CreationTimeUtc,
                    LastWriteTimeUtc = file.LastWriteTimeUtc,
                    Contents = file.Contents
                };
            }

            private void AddWatch()
            {
                if (_fileSystemWatcher != null)
                {
                    if (!_fileSystemWatcher.EnableRaisingEvents)
                        _fileSystemWatcher.EnableRaisingEvents = true;
                    return;
                }

                string directoryName = Path.GetDirectoryName(Options.FileSystemLayoutFilePath);
                string fileName = Path.GetFileName(Options.FileSystemLayoutFilePath);
                _fileSystemWatcher = new FileSystemWatcher(directoryName)
                {
                    Filter = fileName,
                    IncludeSubdirectories = false
                };
                _fileSystemWatcher.Created += OnLayoutFileCreatedOrChanged;
                _fileSystemWatcher.Changed += OnLayoutFileCreatedOrChanged;

                _fileSystemWatcher.EnableRaisingEvents = true;
            }

            private void OnLayoutFileCreatedOrChanged(object sender, FileSystemEventArgs e)
            {
                if (IsSaving || IsLoading) return;

                Thread.Sleep(100);
                Load();
                IsSaving = false;
            }


            void IDisposable.Dispose()
            {
                _fileSystemWatcher?.Dispose();
                _fileSystemWatcher = null;
            }
        }
    }
}