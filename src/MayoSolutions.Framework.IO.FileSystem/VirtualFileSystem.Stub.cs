using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MayoSolutions.Framework.IO.Extensions;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        private class Stub : IDrive, IDirectory, IFile
        {
            private readonly List<VolumeNode> _volumes;
            private readonly char _directorySeparatorChar;
            private readonly FileSystemNodeNavigator _nodeNavigator;

            public Stub(List<VolumeNode> volumes, char directorySeparatorChar, FileSystemNodeNavigator nodeNavigator)
            {
                _volumes = volumes;
                _directorySeparatorChar = directorySeparatorChar;
                _nodeNavigator = nodeNavigator;
            }

            #region IDrive

            public string[] GetDrives()
            {
                return _volumes.Select(v => v.Name).ToArray();
            }

            public VolumeInfo GetVolumeInfo(string drive)
            {
                return _volumes.SingleOrDefault(v => v.StringComparer.Equals(drive.TrimPath(), v.Name.TrimPath()))?.VolumeInfo;
            }

            #endregion

            #region IDirectory

            void IDirectory.CreateDirectory(string path)
            {
                _nodeNavigator.GetOrCreate(_volumes, path, false);
            }

            bool IDirectory.Exists(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                return node != null && node is DirectoryNode;
            }

            void IDirectory.Delete(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (node != null && node is DirectoryNode)
                {
                    var directoryNode = node as DirectoryNode;
                    if (directoryNode.Directories.Any() || directoryNode.Files.Any()) throw new IOException();
                    node.Parent.Directories.Remove(directoryNode);
                }
            }

            DateTime IDirectory.GetLastWriteTimeUtc(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (!(node is DirectoryNode)) throw new IOException();
                return node.LastWriteTimeUtc;
            }
            void IDirectory.Rename(string srcDirectoryName, string destDirectoryName)
            {
                ((IDirectory)this).Move(srcDirectoryName, destDirectoryName);
            }

            void IDirectory.Move(string srcDirectoryName, string destDirectoryName)
            {
                srcDirectoryName = Path.GetFullPath(srcDirectoryName);
                string srcParentFolder = _nodeNavigator.GetParentPath(srcDirectoryName);
                string src = Path.GetFileName(srcDirectoryName);

                string destParentFolder = _nodeNavigator.GetParentPath(destDirectoryName);
                destDirectoryName = Path.GetFullPath(Path.Combine(srcParentFolder, destDirectoryName));
                string dest = Path.GetFileName(destDirectoryName);

                ContainerNode destParent = (ContainerNode)_nodeNavigator.GetOrCreate(_volumes, destParentFolder, false);
                ContainerNode srcParent = (ContainerNode)_nodeNavigator.Get(_volumes, srcParentFolder);

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
            void IDirectory.Delete(string path, bool recursive)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (node != null && node is DirectoryNode) node.Parent.Directories.Remove(node as DirectoryNode);
            }

            string[] IDirectory.GetDirectories(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                return (node as ContainerNode)?.Directories?
                       .Select(x => x.FullName)
                       .OrderBy(x => x, StringComparer.Ordinal)
                       .ToArray() ?? new string[0];
            }

            string[] IDirectory.GetFiles(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                return (node as ContainerNode)?.Files?
                       .Select(x => x.FullName)
                       .OrderBy(x => x, StringComparer.Ordinal)
                       .ToArray() ?? new string[0];
            }

            string[] IDirectory.GetFiles(string path, string searchPattern)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                return (node as DirectoryNode)?.Files?
                       .Select(x => x.FullName)
                       // TODO: Implement searchPattern
                       .OrderBy(x => x, StringComparer.Ordinal)
                       .ToArray() ?? new string[0];
            }

            string[] IDirectory.GetFiles(string path, string searchPattern, SearchOption searchOption)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                return (node as DirectoryNode)?.Files?
                       .Select(x => x.Name)
                       // TODO: Implement searchPattern
                       .OrderBy(x => x, StringComparer.Ordinal)
                       .ToArray() ?? new string[0];
            }

            void IDirectory.SetCreationTime(string path, DateTime creationTime)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (!(node is DirectoryNode)) throw new IOException();
                node.CreationTime = creationTime;
            }

            void IDirectory.SetCreationTimeUtc(string path, DateTime creationTimeUtc)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (!(node is DirectoryNode)) throw new IOException();
                node.CreationTime = creationTimeUtc;
            }

            DateTime IDirectory.GetCreationTime(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (!(node is DirectoryNode)) throw new IOException();
                return node.CreationTime;
            }

            DateTime IDirectory.GetCreationTimeUtc(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (!(node is DirectoryNode)) throw new IOException();
                return node.CreationTimeUtc;
            }

            void IDirectory.SetLastWriteTime(string path, DateTime lastWriteTime)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (node is DirectoryNode) node.LastWriteTime = lastWriteTime;
            }

            void IDirectory.SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (node is DirectoryNode) node.LastWriteTimeUtc = lastWriteTimeUtc;
            }

            DateTime IDirectory.GetLastWriteTime(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                if (!(node is DirectoryNode)) throw new IOException();
                return node.LastWriteTime;
            }

            #endregion

            #region IFIle

            string IFile.ReadAllText(string path)
            {
                return ((IFile)this).ReadAllText(path, Encoding.Default);
            }

            string IFile.ReadAllText(string path, Encoding encoding)
            {
                string[] pathNodes = FileSystemUtility.ParsePath(path, _directorySeparatorChar);
                string parentPath = _nodeNavigator.GetParentPath(pathNodes);
                var parentNode = _nodeNavigator.Get(_volumes, parentPath);
                if (parentNode == null || (!(parentNode is DirectoryNode))) throw new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");
                var node = _nodeNavigator.Get(_volumes, path);
                if (node == null || (!(node is FileNode))) throw new FileNotFoundException($"Could not find file '{path}'.");
                return encoding.GetString(((FileNode)node).Contents);
            }

            bool IFile.Exists(string path)
            {
                var node = _nodeNavigator.Get(_volumes, path);
                return (node != null && node is FileNode);
            }

            void IFile.WriteAllText(string path, string contents)
            {
                ((IFile)this).WriteAllText(path, contents, Encoding.Default);
            }

            void IFile.WriteAllText(string path, string contents, Encoding encoding)
            {
                var pathNodes = FileSystemUtility.ParsePath(path, _directorySeparatorChar);
                var parentNode = AssertParentDirectoryExists(path);
                var node = _nodeNavigator.Get(_volumes, path);
                FileNode fileNode = null;
                if (node == null)
                {
                    fileNode = new FileNode(_nodeNavigator)
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

                var now = DateTime.Now;
                fileNode.LastWriteTime = now;
                if (!(parentNode is VolumeNode) && parentNode != null) parentNode.LastWriteTime = now;
            }

            void IFile.Delete(string path)
            {
                AssertParentDirectoryExists(path);
                var node = _nodeNavigator.Get(_volumes, path);
                if (node != null && node is FileNode && node.Parent != null)
                {
                    var now = DateTime.Now;
                    if (!(node.Parent is VolumeNode)) node.Parent.LastWriteTime = now;
                    node.Parent.Files.Remove(node as FileNode);
                }
            }

            Stream IFile.Open(string path, FileMode fileMode)
            {
                // TODO: Match exceptions to actual fileSystem

                var node = _nodeNavigator.Get(_volumes, path);
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
                    node = _nodeNavigator.GetOrCreate(_volumes, path, true);
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

            void IFile.Rename(string srcFileName, string destFileName)
            {
                ((IFile)this).Move(srcFileName, destFileName);
            }


            void IFile.Move(string srcFileName, string destFileName)
            {
                srcFileName = Path.GetFullPath(srcFileName);
                string srcParentFolder = _nodeNavigator.GetParentPath(srcFileName);
                string src = Path.GetFileName(srcFileName);

                destFileName = Path.GetFullPath(Path.Combine(srcParentFolder, destFileName));
                string destParentFolder = _nodeNavigator.GetParentPath(destFileName);
                string dest = Path.GetFileName(destFileName);

                ContainerNode destParent = (ContainerNode)_nodeNavigator.GetOrCreate(_volumes, destParentFolder, false);
                ContainerNode srcParent = (ContainerNode)_nodeNavigator.Get(_volumes, srcParentFolder);

                if (ReferenceEquals(srcParent, destParent))
                {
                    if (destParent.StringComparer.Equals(src, dest))
                        throw new IOException("Source and destination path must be different.");

                    if (destParent.Files.Contains(src))
                        destParent.Files[src].Name = dest;
                    else
                        destParent.Files.Add(new FileNode(_nodeNavigator) { Name = dest });
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

            DateTime IFile.GetLastWriteTime(string fileName)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                return node.LastWriteTime;
            }

            DateTime IFile.GetLastWriteTimeUtc(string fileName)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                return node.LastWriteTimeUtc;
            }

            void IFile.SetCreationTimeUtc(string fileName, DateTime creationTimeUtc)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                node.CreationTime = creationTimeUtc;
            }

            DateTime IFile.GetCreationTime(string fileName)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                return node.CreationTime;
            }

            DateTime IFile.GetCreationTimeUtc(string fileName)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                return node.CreationTimeUtc;
            }

            void IFile.SetLastWriteTime(string fileName, DateTime lastWriteTime)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                node.LastWriteTime = lastWriteTime;
            }

            void IFile.SetLastWriteTimeUtc(string fileName, DateTime lastWriteTimeUtc)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                node.LastWriteTimeUtc = lastWriteTimeUtc;
            }

            void IFile.SetCreationTime(string fileName, DateTime creationTime)
            {
                var node = _nodeNavigator.Get(_volumes, fileName);
                if (!(node is FileNode)) throw new IOException();
                node.CreationTime = creationTime;
            }

            #endregion




            private ContainerNode AssertParentDirectoryExists(string path)
            {
                string[] pathNodes = FileSystemUtility.ParsePath(path, _directorySeparatorChar);
                string parentPath = _nodeNavigator.GetParentPath(pathNodes);
                var parentNode = _nodeNavigator.Get(_volumes, parentPath);
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
