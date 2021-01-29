using System.Diagnostics;
using System.IO;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        [DebuggerDisplay("Root: {" + nameof(Name) + ",nq}")]
        protected class RootNode : VolumeNode
        {
            public RootNode()
                : base(new VolumeInfo
                {
                    IsCaseSensitive = true,
                    DriveType = DriveType.NoRootDirectory,
                    SupportsHardLinks = true,
                    RootPathName = "/",
                    IsReady = true
                })
            {
            }
        }
    }
}