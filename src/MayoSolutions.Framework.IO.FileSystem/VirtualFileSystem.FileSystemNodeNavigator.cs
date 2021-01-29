using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        protected class FileSystemNodeNavigator
        {
            private readonly char _directorySeparatorChar;

            public FileSystemNodeNavigator(char directorySeparatorChar)
            {
                _directorySeparatorChar = directorySeparatorChar;
            }

            public FileSystemNode Get(List<VolumeNode> volumes, string path)
            {
                return GetOrCreateInternal(volumes, path, false, false);
            }

            public FileSystemNode GetOrCreate(List<VolumeNode> volumes, string path, bool createFile)
            {
                return GetOrCreateInternal(volumes, path, true, createFile);
            }

            public VolumeNode GetOrCreateVolume(List<VolumeNode> volumes, string rootPathName, bool shouldCreate)
            {
                foreach (VolumeNode volume in volumes)
                {
                    if (volume.StringComparer.Equals(volume.Name, rootPathName)) return volume;
                }

                if (shouldCreate)
                {
                    VolumeNode volume;
                    if (_directorySeparatorChar == '/' || rootPathName == "/")
                    {
                        RootNode root = new RootNode();
                        volume = root;
                    }
                    else
                    {
                        volume = new VolumeNode(rootPathName, GetStringComparer(IsVolumeCaseSensitive(rootPathName)));
                    }

                    volume.LastWriteTime = DateTime.Now;
                    volumes.Add(volume);
                    return volume;
                }

                return null;
            }

            private FileSystemNode GetOrCreateInternal(List<VolumeNode> volumes, string path, bool shouldCreate, bool isContextOfFile)
            {
                path = Path.GetFullPath(path);
                string[] pathNodes = FileSystemUtility.ParsePath(path, _directorySeparatorChar);

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

            public string GetParentPath(string path)
            {
                return GetParentPath(path, _directorySeparatorChar);
            }

            public string GetParentPath(string[] pathNodes)
            {
                return GetParentPath(pathNodes, _directorySeparatorChar);
            }

            public static string GetParentPath(string path, char directorySeparatorChar)
            {
                var pathNodes = FileSystemUtility.ParsePath(path, directorySeparatorChar);
                return GetParentPath(pathNodes, directorySeparatorChar);
            }
            public static string GetParentPath(string[] pathNodes, char directorySeparatorChar)
            {
                if (directorySeparatorChar == '/' && pathNodes[0] == "/")
                    return "/" + string.Join(directorySeparatorChar.ToString(), pathNodes.Skip(1).Take(pathNodes.Length - 2).ToArray());

                return string.Join(directorySeparatorChar.ToString(), pathNodes.Take(pathNodes.Length - 1).ToArray());
            }

        }
    }
}
