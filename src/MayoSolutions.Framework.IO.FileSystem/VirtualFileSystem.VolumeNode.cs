using System;
using System.Diagnostics;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        [DebuggerDisplay("Drive: {" + nameof(Name) + ",nq}")]
        private class VolumeNode : ContainerNode
        {
            public VolumeNode(string name, StringComparer stringComparer) : base(name, stringComparer)
            {
            }
        }
    }
}
