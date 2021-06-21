using System;

namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem
    {
        internal abstract class FileSystemPersistenceNode
        {
            public string Name { get; set; }
            public DateTime? LastWriteTimeUtc { get; set; }
            public DateTime? CreationTimeUtc { get; set; }
        }
    }
}