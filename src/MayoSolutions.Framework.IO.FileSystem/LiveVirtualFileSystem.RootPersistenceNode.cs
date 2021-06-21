using System.Collections.Generic;

namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem
    {
        internal class RootPersistenceNode
        {
            public string Platform { get; set; }
            public List<VolumePersistenceNode> Volumes { get; set; }
        }
    }
}