using System;
using System.Diagnostics;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        [DebuggerDisplay("Directory: {" + nameof(Name) + ",nq}")]
        private class DirectoryNode : ContainerNode
        {
            public DirectoryNode(string name, StringComparer stringComparer) : base(name, stringComparer)
            {
            }
        }
    }
}
