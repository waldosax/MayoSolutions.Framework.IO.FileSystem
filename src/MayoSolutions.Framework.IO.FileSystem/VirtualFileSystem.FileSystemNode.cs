using System;
using System.IO;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        protected abstract class FileSystemNode
        {
            private string _name;
            public string Name
            {
                get => _name;
                internal set
                {
                    _name = value;
                    Invalidate();
                }
            }

            private string _fullName;
            public string FullName
            {
                get
                {
                    if (_fullName == null)
                    {
                        string fullName = Name;
                        ContainerNode parent = Parent;
                        while (parent != null)
                        {
                            fullName = parent.Name + Path.DirectorySeparatorChar + fullName;
                            parent = parent.Parent;
                        }

                        _fullName = fullName;
                    }

                    return _fullName;
                }
            }

            public DateTime LastWriteTime
            {
                get => LastWriteTimeUtc.ToLocalTime();
                set => LastWriteTimeUtc = value.ToUniversalTime();
            }

            public DateTime LastWriteTimeUtc { get; set; }
            public DateTime CreationTime
            {
                get => CreationTimeUtc.ToLocalTime();
                set => CreationTimeUtc = value.ToUniversalTime();
            }

            public DateTime CreationTimeUtc { get; set; }

            private ContainerNode _parent;
            public ContainerNode Parent
            {
                get => _parent;
                internal set
                {
                    _parent = value;
                    Invalidate();
                }
            }



            protected FileSystemNode()
                : this(DateTime.UtcNow)
            { }

            protected FileSystemNode(DateTime creationTimeUtc)
            {
                CreationTimeUtc = creationTimeUtc.ToUniversalTime();
            }






            internal virtual void Invalidate()
            {
                _fullName = null;
            }
        }
    }
}
