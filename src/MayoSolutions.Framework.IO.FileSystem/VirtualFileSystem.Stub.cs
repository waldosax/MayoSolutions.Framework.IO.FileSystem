using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
		private class Stub : IDirectory, IFile
		{
			private readonly List<VolumeNode> _volumes;

			public Stub(List<VolumeNode> volumes)
			{
				_volumes = volumes;
			}

			#region IDirectory

			void IDirectory.CreateDirectory(string path)
			{
				FileSystemNodeNavigator.GetOrCreate(_volumes, path, false);
			}

			bool IDirectory.Exists(string path)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				return node != null && node is DirectoryNode;
			}

			void IDirectory.Delete(string path)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				if (node != null && node is DirectoryNode)
				{
					var directoryNode = node as DirectoryNode;
					if (directoryNode.Directories.Any() || directoryNode.Files.Any()) throw new IOException();
					node.Parent.Directories.Remove(directoryNode);
				}
			}

			void IFile.Rename(string srcFileName, string destFileName)
			{
				((IFile)this).Move(srcFileName, destFileName);
			}

			void IDirectory.Rename(string srcDirectoryName, string destDirectoryName)
			{
				((IDirectory)this).Move(srcDirectoryName, destDirectoryName);
			}

			void IDirectory.Move(string srcDirectoryName, string destDirectoryName)
			{
				srcDirectoryName = Path.GetFullPath(srcDirectoryName);
				string srcParentFolder = FileSystemNodeNavigator.GetParentPath(srcDirectoryName);
				string src = Path.GetFileName(srcDirectoryName);

				string destParentFolder = FileSystemNodeNavigator.GetParentPath(destDirectoryName);
				destDirectoryName = Path.GetFullPath(Path.Combine(srcParentFolder, destDirectoryName));
				string dest = Path.GetFileName(destDirectoryName);

				ContainerNode destParent = (ContainerNode)FileSystemNodeNavigator.GetOrCreate(_volumes, destParentFolder, false);
				ContainerNode srcParent = (ContainerNode)FileSystemNodeNavigator.Get(_volumes, srcParentFolder);

				if (ReferenceEquals(srcParent, destParent))
				{
					if (destParent.StringComparer.Equals(src, dest)) return;    // TODO: Throw Exception?
					destParent.Directories[src].Name = dest;
				}
				else
				{
					DirectoryNode srcDirectory = srcParent.Directories[src];
					DirectoryNode destDirectory = destParent.Directories[dest];

					if (destDirectory != null) throw new IOException("Cannot create a file when that file already exists.");

					destDirectory = srcDirectory;
					srcParent.Directories.Remove(srcDirectory);
					destParent.Directories.Add(srcDirectory);
					destDirectory.Name = dest;
				}
			}

			void IFile.Move(string srcFileName, string destFileName)
			{
				srcFileName = Path.GetFullPath(srcFileName);
				string srcParentFolder = FileSystemNodeNavigator.GetParentPath(srcFileName);
				string src = Path.GetFileName(srcFileName);

				destFileName = Path.GetFullPath(Path.Combine(srcParentFolder, destFileName));
				string destParentFolder = FileSystemNodeNavigator.GetParentPath(destFileName);
				string dest = Path.GetFileName(destFileName);

				ContainerNode destParent = (ContainerNode)FileSystemNodeNavigator.GetOrCreate(_volumes, destParentFolder, false);
				ContainerNode srcParent = (ContainerNode)FileSystemNodeNavigator.Get(_volumes, srcParentFolder);

				if (ReferenceEquals(srcParent, destParent))
                {
                    if (destParent.StringComparer.Equals(src, dest))
                        throw new IOException("Source and destination path must be different.");

                    if (destParent.Files.Contains(src))
                        destParent.Files[src].Name = dest;
                    else
                        destParent.Files.Add(new FileNode {Name = dest});
                }
				else
				{
					FileNode srcFile = srcParent.Files[src];
					FileNode destFile = destParent.Files[dest];

					// TODO: if (srcFile == null) throw new IOException("Cannot create a file when that file already exists.");
					if (destFile != null) throw new IOException("Cannot create a file when that file already exists.");

					destFile = srcFile;
					srcParent.Files.Remove(srcFile);
					destParent.Files.Add(destFile);
					destFile.Name = dest;
				}
			}

			void IDirectory.Delete(string path, bool recursive)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				if (node != null && node is DirectoryNode) node.Parent.Directories.Remove(node as DirectoryNode);
			}

			string[] IDirectory.GetDirectories(string path)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				return (node as DirectoryNode)?.Directories?
					   .Select(x => x.FullName)
					   .OrderBy(x => x, StringComparer.Ordinal)
					   .ToArray() ?? new string[0];
			}

			string[] IDirectory.GetFiles(string path)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				return (node as DirectoryNode)?.Files?
					   .Select(x => x.FullName)
					   .OrderBy(x => x, StringComparer.Ordinal)
					   .ToArray() ?? new string[0];
			}

			string[] IDirectory.GetFiles(string path, string searchPattern)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				return (node as DirectoryNode)?.Files?
					   .Select(x => x.FullName)
					   // TODO: Implement searchPattern
					   .OrderBy(x => x, StringComparer.Ordinal)
					   .ToArray() ?? new string[0];
			}

			string[] IDirectory.GetFiles(string path, string searchPattern, SearchOption searchOption)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				return (node as DirectoryNode)?.Files?
					   .Select(x => x.Name)
					   // TODO: Implement searchPattern
					   .OrderBy(x => x, StringComparer.Ordinal)
					   .ToArray() ?? new string[0];
			}

			void IDirectory.SetLastWriteTime(string path, DateTime lastWriteTime)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				if (node != null && node is DirectoryNode) node.LastWriteTime = lastWriteTime;
			}

			void IDirectory.SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				if (node != null && node is DirectoryNode) node.LastWriteTimeUtc = lastWriteTimeUtc;
			}

			#endregion

			#region IFIle

			string IFile.ReadAllText(string path)
			{
				return ((IFile)this).ReadAllText(path, Encoding.Default);
			}

			string IFile.ReadAllText(string path, Encoding encoding)
			{
				string[] pathNodes = FileSystemNodeNavigator.ParsePath(path);
				string parentPath = FileSystemNodeNavigator.GetParentPath(pathNodes);
				var parentNode = FileSystemNodeNavigator.Get(_volumes, parentPath);
				if (parentNode == null || (!(parentNode is DirectoryNode))) throw new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				if (node == null || (!(node is FileNode))) throw new FileNotFoundException($"Could not find file '{path}'.");
				return encoding.GetString(((FileNode)node).Contents);
			}

			bool IFile.Exists(string path)
			{
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				return (node != null && node is FileNode);
			}

			void IFile.WriteAllText(string path, string contents)
			{
				((IFile)this).WriteAllText(path, contents, Encoding.Default);
			}

			void IFile.WriteAllText(string path, string contents, Encoding encoding)
			{
				var pathNodes = FileSystemNodeNavigator.ParsePath(path);
				var parentNode = AssertParentDirectoryExists(path);
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				FileNode fileNode = null;
				if (node == null)
				{
					fileNode = new FileNode
					{
						Name = pathNodes.LastOrDefault(),
						LastWriteTime = DateTime.Now,
						Parent = parentNode,
					};
					parentNode.Files.Add(fileNode);
				}
				else if (!(node is FileNode)) throw new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");
				else fileNode = node as FileNode;

				fileNode.Contents = encoding.GetBytes(contents);
				fileNode.LastWriteTime = DateTime.Now;
			}

			void IFile.Delete(string path)
			{
				AssertParentDirectoryExists(path);
				var node = FileSystemNodeNavigator.Get(_volumes, path);
				if (node != null && node is FileNode) node.Parent.Files.Remove(node as FileNode);
			}

			Stream IFile.Open(string path, FileMode fileMode)
			{
				// TODO: Match exceptions to actual fileSystem

				var node = FileSystemNodeNavigator.Get(_volumes, path);
				if (node != null && !(node is FileNode))
				{
					throw new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");
				}

				if (node != null && fileMode == FileMode.CreateNew)
				{
					throw new IOException();
				}

				if (node == null && fileMode == FileMode.Append)
				{
					throw new IOException();
				}

				if (node == null)
				{
					node = FileSystemNodeNavigator.GetOrCreate(_volumes, path, true);
				}
				return new FileNodeStream((FileNode)node, fileMode);
			}

			Stream IFile.Open(string path, FileMode fileMode, FileAccess fileAccess)
			{
				return ((IFile)this).Open(path, fileMode);
			}

			Stream IFile.Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
			{
				return ((IFile)this).Open(path, fileMode);
			}

			#endregion

			private ContainerNode AssertParentDirectoryExists(string path)
			{
				string[] pathNodes = FileSystemNodeNavigator.ParsePath(path);
				string parentPath = FileSystemNodeNavigator.GetParentPath(pathNodes);
				var parentNode = FileSystemNodeNavigator.Get(_volumes, parentPath);
				if (parentNode == null || (!(parentNode is ContainerNode))) throw new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");
				return parentNode as ContainerNode;
			}

			private class FileNodeStream : MemoryStream
			{
				private readonly FileNode _node;

				public FileNodeStream(FileNode node, FileMode fileMode)
				{
					_node = node;

					switch (fileMode)
					{
						case FileMode.Open:
						case FileMode.OpenOrCreate:
							Write(node.Contents, 0, node.Contents.Length);
							Position = 0L;
							break;
						case FileMode.Append:
							Write(node.Contents, 0, node.Contents.Length);
							break;
					}
				}

				public override void Flush()
				{
					_node.Contents = ToArray();
				}

				protected override void Dispose(bool disposing)
				{
					if (disposing)
					{
						Flush();
					}
					base.Dispose(disposing);
				}
			}
		}
	}
}
