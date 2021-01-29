using System;
using System.Diagnostics;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        [DebuggerDisplay("Directory: {" + nameof(Name) + ",nq}")]
        protected class DirectoryNode : ContainerNode
        {
            public DirectoryNode(
                FileSystemNodeNavigator nodeNavigator, 
                string name, 
                StringComparer stringComparer,
                DateTime? creationTimUtc = null
                )
                : base(nodeNavigator, name, stringComparer, creationTimUtc)
            {
            }
        }
    }
}
