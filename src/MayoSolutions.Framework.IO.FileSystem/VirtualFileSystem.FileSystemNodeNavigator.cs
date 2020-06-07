using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
		private static class FileSystemNodeNavigator
		{
			public static FileSystemNode Get(List<VolumeNode> volumes, string path)
			{
				return GetOrCreateInternal(volumes, path, false, false);
			}

			public static FileSystemNode GetOrCreate(List<VolumeNode> volumes, string path, bool createFile)
			{
				return GetOrCreateInternal(volumes, path, true, createFile);
			}

			public static VolumeNode GetOrCreateVolume(List<VolumeNode> volumes, string rootPathName, bool shouldCreate)
			{
				foreach (VolumeNode volume in volumes)
				{
					if (volume.StringComparer.Equals(volume.Name, rootPathName)) return volume;
				}

				if (shouldCreate)
				{
					VolumeNode volume = new VolumeNode(rootPathName, GetStringComparer(IsVolumeCaseSensitive(rootPathName)))
					{
						LastWriteTime = DateTime.Now
					};
					volumes.Add(volume);
					return volume;
				}

				return null;
			}

			private static FileSystemNode GetOrCreateInternal(List<VolumeNode> volumes, string path, bool shouldCreate, bool isContextOfFile)
			{
				path = Path.GetFullPath(path);
				string[] pathNodes = ParsePath(path);

				VolumeNode volume = GetOrCreateVolume(volumes, pathNodes[0], shouldCreate);
				if (volume == null) return null;
				StringComparer stringComparer = volume.StringComparer;

				ContainerNode current = volume;
				if (pathNodes.Length <= 1) return current;

				int i;
				for (i = 1; i < pathNodes.Length - 1; i++)
				{
					bool found = false;

					var child = current.Directories[pathNodes[i]];
					if (child != null)
					{
						found = true;
						current = child;
					}

					if (!found)
					{
						if (!shouldCreate) return null;
						break;
					}
				}

				if (i >= pathNodes.Length && Debugger.IsAttached) Debugger.Break();
				var pathNode = pathNodes[i];
				var directory = current.Directories[pathNode];
				if (directory != null) return directory;

				var file = current.Files[pathNode];
				if (file != null) return file;

				if (!shouldCreate) return null;

				for (int j = i; j < pathNodes.Length - 1; j++)
				{
					DirectoryNode existingDirectory = current.Directories[pathNodes[i]];
					if (existingDirectory == null)
					{
						DirectoryNode newDirectory = new DirectoryNode(pathNodes[i], stringComparer)
						{
							LastWriteTime = DateTime.Now,
						};
						current.Directories.Add(newDirectory);
						current = newDirectory;
					}
					else
					{
						current = existingDirectory;
					}
				}

				if (!isContextOfFile)
				{
					DirectoryNode existingDirectory = current.Directories[pathNodes[i]];
					if (existingDirectory == null)
					{
						DirectoryNode newDirectory = new DirectoryNode(pathNodes[i], stringComparer)
						{
							LastWriteTime = DateTime.Now,
						};
						current.Directories.Add(newDirectory);
						return newDirectory;
					}

					return existingDirectory;
				}

				FileNode existingFile = current.Files[pathNodes[i]];
				if (existingFile == null)
				{
					FileNode fileNode = new FileNode
					{
						Name = pathNodes[i],
						LastWriteTime = DateTime.Now,
					};
					current.Files.Add(fileNode);
					return fileNode;
				}

				return existingFile;
			}

			public static string[] ParsePath(string path)
			{
				List<string> pathNodes = new List<string>();
				string tmp = string.Copy(path);
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

			public static string GetParentPath(string path)
			{
				var pathNodes = ParsePath(path);
				return string.Join(Path.DirectorySeparatorChar.ToString(), pathNodes.Take(pathNodes.Length - 1).ToArray());
			}
			public static string GetParentPath(string[] pathNodes)
			{
				return string.Join(Path.DirectorySeparatorChar.ToString(), pathNodes.Take(pathNodes.Length - 1).ToArray());
			}

		}
	}
}
