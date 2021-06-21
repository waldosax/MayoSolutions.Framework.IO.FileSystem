using System.Collections.Generic;
using System.Diagnostics;

namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem
    {
        [DebuggerDisplay("{" + nameof(Name) + ",nq}")]
        internal class FilePersistenceNode : FileSystemPersistenceNode
        {
            public byte[] Contents { get; set; }
        }
    }
}