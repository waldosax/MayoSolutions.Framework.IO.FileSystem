using System;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        protected abstract class FileSystemNode
        {
            internal readonly FileSystemNodeNavigator NodeNavigator;
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
            public string FullName => _fullName ?? (_fullName = NodeNavigator.GetFullPath(this));

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



            

            protected FileSystemNode(FileSystemNodeNavigator nodeNavigator, DateTime? creationTimeUtc = null)
            {
                NodeNavigator = nodeNavigator;
                CreationTimeUtc = (creationTimeUtc ?? DateTime.UtcNow).ToUniversalTime();
            }
            




            internal virtual void Invalidate()
            {
                _fullName = null;
            }
        }
    }
}
