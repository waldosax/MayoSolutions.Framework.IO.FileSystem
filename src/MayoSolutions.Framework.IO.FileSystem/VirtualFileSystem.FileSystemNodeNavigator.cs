﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MayoSolutions.Framework.IO.Extensions;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        protected class FileSystemNodeNavigator
        {
            private readonly StringComparer _pathComparer;
            private readonly char _directorySeparatorChar;

            internal FileSystemNodeNavigator(StringComparer pathComparer, char directorySeparatorChar)
            {
                _pathComparer = pathComparer;
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

            public VolumeNode GetOrCreateVolume(List<VolumeNode> volumes, VolumeInfo volumeInfo, bool shouldCreate)
            {
                foreach (VolumeNode volume in volumes)
                {
                    if (volume.StringComparer.Equals(volume.Name, volumeInfo.RootPathName)) return volume;
                }

                if (shouldCreate)
                {
                    VolumeNode volume = new VolumeNode(this, volumeInfo)
                    {
                        LastWriteTime = DateTime.Now
                    };
                    volumes.Add(volume);
                    return volume;
                }

                return null;
            }
            public VolumeNode GetOrCreateVolume(List<VolumeNode> volumes, string rootPathName, bool shouldCreate)
            {
                return GetOrCreateVolume(volumes, new VolumeInfo
                {
                    RootPathName = rootPathName,
                    DriveType = DriveType.Fixed,
                    DriveFormat = "NTFS",
                    IsReady = true
                }, shouldCreate);
            }

            private FileSystemNode GetOrCreateInternal(List<VolumeNode> volumes, string path, bool shouldCreate, bool isContextOfFile)
            {
                if (_directorySeparatorChar == '\\')
                {
                    if (!isContextOfFile) { path = path.TrimPath() + _directorySeparatorChar; }
                    path = Path.GetFullPath(path);
                }
                if (!isContextOfFile) { path = path.TrimPath(); }
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

                for (int j = i; j < pathNodes.Length - 1; j++, i = j)
                {
                    var directoryName = pathNodes[j];
                    DirectoryNode existingDirectory = current.Directories[directoryName];
                    if (existingDirectory == null)
                    {
                        DirectoryNode newDirectory = new DirectoryNode(this, directoryName, stringComparer)
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
                    var directoryName = pathNodes[i];
                    lock (current.Directories.SyncLock)
                    {
                        DirectoryNode existingDirectory = current.Directories[directoryName];
                        if (existingDirectory == null)
                        {
                            DirectoryNode newDirectory = new DirectoryNode(this, directoryName, stringComparer)
                            {
                                LastWriteTime = DateTime.Now,
                            };
                            current.Directories.Add(newDirectory);
                            return newDirectory;
                        }

                        return existingDirectory;
                    }
                }

                var fileName = pathNodes[i];
                lock (current.Files.SyncLock)
                {
                    FileNode existingFile = current.Files[fileName];
                    if (existingFile == null)
                    {
                        FileNode fileNode = new FileNode(this)
                        {
                            Name = fileName,
                            LastWriteTime = DateTime.Now,
                        };
                        current.Files.Add(fileNode);
                        return fileNode;
                    }

                    return existingFile;
                }
            }

            public string GetFullPath(FileSystemNode node)
            {
                List<FileSystemNode> nodes = new List<FileSystemNode> { node };
                var current = node;
                while (current.Parent != null)
                {
                    nodes.Insert(0, current.Parent);
                    current = current.Parent;
                }

                var firstNode = nodes[0];
                if (firstNode is RootNode) return "/" + string.Join(_directorySeparatorChar.ToString(), nodes.Skip(1).Take(nodes.Count - 1).Select(nd => nd.Name).ToArray());
                return string.Join(_directorySeparatorChar.ToString(), nodes.Take(nodes.Count).Select(nd => nd.Name).ToArray());
            }
            public string GetFullPath(string path)
            {
                if (_directorySeparatorChar == '\\') return Path.GetFullPath(path);
                return path;
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
