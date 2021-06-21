using System;
using System.Diagnostics;
using System.IO;
using MayoSolutions.Framework.IO.Extensions;

namespace MayoSolutions.Framework.IO
{
    public partial class VirtualFileSystem
    {
        [DebuggerDisplay("Drive: {" + nameof(Name) + ",nq}")]
        protected class VolumeNode : ContainerNode
        {
            public VolumeInfo VolumeInfo { get; }

            public VolumeNode(
                FileSystemNodeNavigator nodeNavigator,
                VolumeInfo volumeInfo
                )
                : base(
                    nodeNavigator,
                    volumeInfo.RootPathName.TrimPath(),
                    volumeInfo.IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
            {
                VolumeInfo = volumeInfo;
            }
        }
    }
}
