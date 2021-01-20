using System.Diagnostics;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        [DebuggerDisplay("File: {" + nameof(Name) + ",nq}")]
        protected class FileNode : FileSystemNode
        {
            private byte[] _contents = new byte[0];
            public byte[] Contents
            {
                get => _contents;
                set => _contents = value ?? new byte[0];
            }
        }
    }
}
