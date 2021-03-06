﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        protected abstract class ContainerNode : FileSystemNode
        {
            internal readonly StringComparer StringComparer;

            public ChildList<DirectoryNode> Directories { get; }
            public ChildList<FileNode> Files { get; }

            protected ContainerNode(FileSystemNodeNavigator nodeNavigator, string name, StringComparer stringComparer, DateTime? creationTimeUtc = null)
            : base(nodeNavigator, creationTimeUtc)
            {
                StringComparer = stringComparer;
                Directories = new ChildList<DirectoryNode>(this);
                Files = new ChildList<FileNode>(this);
                Name = name;
            }

            public class ChildList<T> : List<T>
                where T : FileSystemNode
            {
                internal readonly object SyncLock = new object();
                private readonly ContainerNode _parent;

                public ChildList(ContainerNode parent)
                {
                    _parent = parent;
                }

                private T Find(string name)
                {
                    lock (SyncLock)
                    {
                        Func<T, bool> predicate = GetFindPredicate(_parent, name);
                        Debug.Assert(this.Count(predicate) <= 1, $"More than one element found for '{name}' in '{_parent.FullName}'.");
                        try
                        {
                            return this.SingleOrDefault(predicate);
                            //return this.FirstOrDefault(predicate);
                        }
                        catch (InvalidOperationException e)
                        {
                            throw new InvalidOperationException($"More than one element found for '{name}' in '{_parent.FullName}'.", e);
                        }
                    }
                }

                private Func<T, bool> GetFindPredicate(ContainerNode parent, string name)
                {
                    return x => parent.StringComparer.Equals(x.Name, name);
                }

                public T this[string name] => Find(name);

                public new void Add(T item)
                {
                    lock (SyncLock)
                    {
                        item.Parent = _parent;
                        base.Add(item);
                    }
                }

                public void Remove(string name)
                {
                    lock (SyncLock)
                    {
                        T item = Find(name);
                        if (item != null) Remove(item);
                    }
                }

                public new void Remove(T item)
                {
                    lock (SyncLock)
                    {
                        item.Parent = null;
                        base.Remove(item);
                    }
                }

                public bool Contains(string name)
                {
                    return Find(name) != null;
                }
            }

            internal override void Invalidate()
            {
                base.Invalidate();
                Directories.ForEach(x => x.Invalidate());
                Files.ForEach(x => x.Invalidate());
            }
        }
    }
}
