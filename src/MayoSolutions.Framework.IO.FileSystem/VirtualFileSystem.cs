using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MayoSolutions.Framework.IO
{
	// NOTE: Somebody got their recursion hard-on! Oh, wait. It was me.
	public partial class VirtualFileSystem : IFileSystem
	{
		public IDrive Drive { get; }
		public IDirectory Directory { get; }
		public IFile File { get; }

		private readonly List<VolumeNode> _volumes;

		private static bool IsOperatingSystemCaseSensitive()
		{
			// TODO: Support for non-Windows (dotnetcore)
			string winDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
			string winDrive = winDir.Split(Path.DirectorySeparatorChar).First() + Path.DirectorySeparatorChar;
			return IsVolumeCaseSensitive(winDrive);
		}

		// https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getvolumeinformationa
		private static bool IsVolumeCaseSensitive(string rootPathName)
		{
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

		private static StringComparer GetStringComparer(bool caseSensitive)
		{
			return caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
		}


		public VirtualFileSystem()
		{
			_volumes = new List<VolumeNode>();
			Stub stub = new Stub(_volumes);
			Drive = stub;
			Directory = stub;
			File = stub;
		}

		public static VirtualFileSystem FromPhysicalPath(string path)
		{
			VirtualFileSystem vfs = new VirtualFileSystem();

			vfs = vfs.AndPhysicalPath(path);

			return vfs;
		}

		public VirtualFileSystem WithFiles(string[] paths)
		{
			foreach (string path in paths)
			{
				WithFileInternal(path, null);
			}
			return this;
		}
		public VirtualFileSystem WithFile(string path, DateTime? lastWriteTime = null)
		{
			return WithFileInternal(path, null, lastWriteTime);
		}

		public VirtualFileSystem WithFile(string path, string contents, DateTime? lastWriteTime = null)
		{
			return WithFile(path, contents, Encoding.Default, lastWriteTime);
		}
		public VirtualFileSystem WithFile(string path, string contents, Encoding encoding, DateTime? lastWriteTime = null)
		{
			return WithFileInternal(path, encoding.GetBytes(contents), lastWriteTime);
		}
		public VirtualFileSystem WithFile(string path, byte[] contents, DateTime? lastWriteTime = null)
		{
			return WithFileInternal(path, contents, lastWriteTime);
		}

		private VirtualFileSystem WithFileInternal(string path, byte[] contents, DateTime? lastWriteTime = null)
		{
			VirtualFileSystem vfs = this;

			path = Path.GetFullPath(path);

			string fileName = Path.GetFileName(path);
			string parentDirectory = FileSystemNodeNavigator.GetParentPath(path);
			vfs = vfs.AndPhysicalPath(parentDirectory);

			ContainerNode parent = (ContainerNode)FileSystemNodeNavigator.Get(vfs._volumes, parentDirectory);
			FileNode fileNode = parent.Files[fileName];
			if (fileNode == null)
			{
				fileNode = new FileNode
				{
					Name = fileName,
					Contents = contents ?? new byte[0],
					LastWriteTime = lastWriteTime ?? DateTime.Now,
				};
				parent.Files.Add(fileNode);
			}

			return vfs;
		}

        public VirtualFileSystem AndPhysicalPath(string path)
        {
            return CreatePathInternal(path, true);
        }

		public static VirtualFileSystem WithPath(string path)
        {
            VirtualFileSystem vfs = new VirtualFileSystem();

            vfs = vfs.AndPath(path);

            return vfs;
        }

		public VirtualFileSystem AndPath(string path)
        {
            return CreatePathInternal(path, false);
        }

        private VirtualFileSystem CreatePathInternal(string path, bool createChildrenFromPhysicalPath)
        {
			VirtualFileSystem vfs = this;

			path = Path.GetFullPath(path);
			string[] pathNodes = FileSystemNodeNavigator.ParsePath(path);

			if (pathNodes.Length == 0) throw new IOException("Path could not be parsed.");

			VolumeNode volume = FileSystemNodeNavigator.GetOrCreateVolume(_volumes, pathNodes[0], true);
			StringComparer stringComparer = volume.StringComparer;

			ContainerNode parent = volume;
			for (int i = 1; i < pathNodes.Length; i++)
			{
				string fullName = string.Join(Path.DirectorySeparatorChar.ToString(), pathNodes, 0, i + 1);
				DirectoryNode existing = parent.Directories[pathNodes[i]];
				if (existing == null)
				{
					DirectoryNode current = new DirectoryNode(pathNodes[i], stringComparer)
					{
						LastWriteTime = new DirectoryInfo(fullName).LastWriteTime,
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

			FileInfo[] files = di.GetFiles();
			foreach (FileInfo fi in files)
			{
				FileNode fileNode = new FileNode
				{
					Name = fi.Name,
					LastWriteTime = fi.LastWriteTime,
				};
				parent.Files.Add(fileNode);
			}

			DirectoryInfo[] directories = di.GetDirectories();
			foreach (DirectoryInfo child in directories)
			{
				DirectoryNode directoryNode = new DirectoryNode(child.Name, parent.StringComparer)
				{
					LastWriteTime = child.LastWriteTime,
				};
				parent.Directories.Add(directoryNode);
				CreateChildrenFromPhysicalPath(child, directoryNode);
			}
		}
	}
}
