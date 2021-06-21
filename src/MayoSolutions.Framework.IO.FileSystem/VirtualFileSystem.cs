using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MayoSolutions.Framework.IO
{
    // NOTE: Somebody got their recursion hard-on! Oh, wait. It was me.
    public partial class VirtualFileSystem : IFileSystem
    {
        public IDrive Drive { get; protected set; }
        public IDirectory Directory { get; protected set; }
        public IFile File { get; protected set; }

        public OSPlatform Platform { get; protected set; }
        public StringComparer PathComparer =>
            StringComparison == StringComparison.OrdinalIgnoreCase
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;
        public StringComparison StringComparison =>
            Platform == OSPlatform.Windows || Platform == OSPlatform.OSX
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
        public char DirectorySeparatorChar { get; protected set; }


        protected List<VolumeNode> Volumes;
        protected FileSystemNodeNavigator NodeNavigator;

        #region Meta

        private static bool IsOperatingSystemCaseSensitive()
        {
            if (OperatingSystem.Platform == OSPlatform.Windows)
            {
                string winDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                string winDrive = winDir.Split(Path.DirectorySeparatorChar).First() + Path.DirectorySeparatorChar;
                return IsVolumeCaseSensitive(winDrive);
            }

            return true;
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getvolumeinformationa
        private static bool IsVolumeCaseSensitive(string rootPathName)
        {
            if (rootPathName == "/") return true;
            if (rootPathName == "//") return true;

            if (rootPathName[rootPathName.Length - 1] == Path.VolumeSeparatorChar) rootPathName += Path.DirectorySeparatorChar;

            StringBuilder volumeNameBuffer = new StringBuilder(261);
            StringBuilder fileSystemNameBuffer = new StringBuilder(261);

            if (Win32Api.GetVolumeInformation(rootPathName,
                volumeNameBuffer, 261, out _,
                out _, out var fileSystemFlags,
                fileSystemNameBuffer, 261))
            {
                return (fileSystemFlags | Win32Api.FileSystemFeature.CaseSensitiveSearch) ==
                       Win32Api.FileSystemFeature.CaseSensitiveSearch;
            }

            return IsOperatingSystemCaseSensitive();
        }

        #endregion

        public VirtualFileSystem()
            : this(OperatingSystem.Platform)
        {
        }

        public VirtualFileSystem(OSPlatform platform)
        {
            CreateInternal(platform);
        }

        protected internal void CreateInternal(OSPlatform platform)
        {
            Platform = platform;
            DirectorySeparatorChar = platform != OSPlatform.Windows ?'/':'\\';
            NodeNavigator = new FileSystemNodeNavigator(PathComparer, DirectorySeparatorChar);
            
            Volumes = new List<VolumeNode>();
            if (platform != OSPlatform.Windows)
            {
                Volumes.Add(new RootNode(NodeNavigator));
            }

            Stub stub = new Stub(Volumes, DirectorySeparatorChar, NodeNavigator);
            Drive = stub;
            Directory = stub;
            File = stub;
        }

        public virtual void Clear()
        {
            Volumes.Clear();
        }
        

        public static VirtualFileSystem FromPhysicalPath(string path)
        {
            VirtualFileSystem vfs = new VirtualFileSystem();

            vfs = vfs.AndPhysicalPath(path);

            return vfs;
        }

        public virtual VirtualFileSystem WithFiles(string[] paths)
        {
            foreach (string path in paths)
            {
                WithFileInternal(path, null);
            }
            return this;
        }
        public virtual VirtualFileSystem WithFile(string path, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            return WithFileInternal(path, null, creationTime, lastWriteTime);
        }

        public virtual VirtualFileSystem WithFile(string path, string contents, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            return WithFile(path, contents, Encoding.Default, creationTime, lastWriteTime);
        }
        public virtual VirtualFileSystem WithFile(string path, string contents, Encoding encoding, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            return WithFileInternal(path, encoding.GetBytes(contents), creationTime, lastWriteTime);
        }
        public virtual VirtualFileSystem WithFile(string path, byte[] contents, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            return WithFileInternal(path, contents, creationTime, lastWriteTime);
        }

        private VirtualFileSystem WithFileInternal(string path, byte[] contents, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            VirtualFileSystem vfs = this;

            if (DirectorySeparatorChar == '\\') path = Path.GetFullPath(path);

            string fileName = Path.GetFileName(path);
            string parentDirectory = NodeNavigator.GetParentPath(path);
            vfs = vfs.AndPath(parentDirectory);

            ContainerNode parent = (ContainerNode)NodeNavigator.Get(vfs.Volumes, parentDirectory);
            FileNode fileNode = parent.Files[fileName];
            if (fileNode == null)
            {
                fileNode = new FileNode(parent.NodeNavigator)
                {
                    Name = fileName,
                    Contents = contents ?? new byte[0],
                    CreationTime = creationTime ?? DateTime.Now,
                    LastWriteTime = lastWriteTime ?? DateTime.Now,
                };
                parent.Files.Add(fileNode);
            }

            return vfs;
        }

        public virtual VirtualFileSystem AndPhysicalPath(string path)
        {
            return CreatePathInternal(path, true);
        }

        public static VirtualFileSystem WithPath(string path, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            VirtualFileSystem vfs = new VirtualFileSystem();

            vfs = vfs.AndPath(path, creationTime, lastWriteTime);

            return vfs;
        }

        public virtual VirtualFileSystem AndPath(string path, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            return CreatePathInternal(path, false, creationTime, lastWriteTime);
        }

        private VirtualFileSystem CreatePathInternal(string path, bool createChildrenFromPhysicalPath, DateTime? creationTime = null, DateTime? lastWriteTime = null)
        {
            VirtualFileSystem vfs = this;

            if (DirectorySeparatorChar == '\\') path = Path.GetFullPath(path);
            path = FileSystemUtility.SanitizePath(path, DirectorySeparatorChar);
            string[] pathNodes = FileSystemUtility.ParsePath(path, DirectorySeparatorChar);

            if (pathNodes.Length == 0) throw new IOException("Path could not be parsed.");

            VolumeNode volume = NodeNavigator.GetOrCreateVolume(Volumes, pathNodes[0], true);
            StringComparer stringComparer = volume.StringComparer;

            ContainerNode parent = volume;
            for (int i = 1; i < pathNodes.Length; i++)
            {
                string fullName = string.Join(Path.DirectorySeparatorChar.ToString(), pathNodes, 0, i + 1);
                DirectoryNode existing = parent.Directories[pathNodes[i]];
                if (existing == null)
                {
                    DirectoryNode current = new DirectoryNode(NodeNavigator, pathNodes[i], stringComparer)
                    {
                        CreationTime = createChildrenFromPhysicalPath ? new DirectoryInfo(fullName).CreationTime: creationTime?.ToLocalTime() ?? DateTime.Now,
                        LastWriteTime = createChildrenFromPhysicalPath ? new DirectoryInfo(fullName).LastWriteTime: lastWriteTime?.ToLocalTime() ?? DateTime.Now,
                    };
                    parent.Directories.Add(current);
                    parent = current;
                }
                else
                {
                    parent = existing;
                }
            }

            if (createChildrenFromPhysicalPath)
            {
                DirectoryInfo di = new DirectoryInfo(path);
                if (!(parent is VolumeNode)) CreateChildrenFromPhysicalPath(di, parent);
            }

            return vfs;
        }

        private static void CreateChildrenFromPhysicalPath(DirectoryInfo di, ContainerNode parent)
        {
            if (!di.Exists) return;
            //

            FileInfo[] files;
            try
            {
                files = di.GetFiles();
            }
            catch (IOException e)
            {
                if (e.Message.StartsWith("The network path was not found.")) files = new FileInfo[0];
                else throw;
            }

            foreach (FileInfo fi in files)
            {
                FileNode fileNode = new FileNode(parent.NodeNavigator)
                {
                    Name = fi.Name,
                    LastWriteTime = fi.LastWriteTime,
                };
                parent.Files.Add(fileNode);
            }

            DirectoryInfo[] directories;
            try
            {
                directories = di.GetDirectories();
            }
            catch (IOException e)
            {
                if (e.Message.StartsWith("The network path was not found.")) directories = new DirectoryInfo[0];
                else throw;
            }

            foreach (DirectoryInfo child in directories)
            {
                DirectoryNode directoryNode = new DirectoryNode(parent.NodeNavigator, child.Name, parent.StringComparer)
                {
                    LastWriteTime = child.LastWriteTime,
                };
                parent.Directories.Add(directoryNode);
                CreateChildrenFromPhysicalPath(child, directoryNode);
            }
        }
    }
}
