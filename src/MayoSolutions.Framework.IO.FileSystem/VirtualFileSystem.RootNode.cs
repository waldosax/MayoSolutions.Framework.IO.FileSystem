using System.Diagnostics;
using System.IO;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        [DebuggerDisplay("Root: {" + nameof(Name) + ",nq}")]
        protected class RootNode : VolumeNode
        {
            public RootNode(FileSystemNodeNavigator nodeNavigator)
                : base(nodeNavigator, new VolumeInfo    // TODO: Default VolumeInfo
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