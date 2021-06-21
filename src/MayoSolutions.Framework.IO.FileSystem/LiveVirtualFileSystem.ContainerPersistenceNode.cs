using System.Collections.Generic;

namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem
    {
        internal class ContainerPersistenceNode : FileSystemPersistenceNode
        {
            public List<DirectoryPersistenceNode> Directories { get; set; }
            public List<FilePersistenceNode> Files { get; set; }

        }
    }
}