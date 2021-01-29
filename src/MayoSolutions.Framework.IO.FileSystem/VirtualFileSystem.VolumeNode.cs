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

            public VolumeNode(
                FileSystemNodeNavigator nodeNavigator,
                string name, 
                StringComparer stringComparer)
                : base(nodeNavigator, name, stringComparer)
            {
                VolumeInfo = new VolumeInfo
                {
                    RootPathName = name,
                    IsCaseSensitive = (
                        stringComparer == StringComparer.CurrentCulture ||
                        stringComparer == StringComparer.InvariantCulture ||
                        stringComparer == StringComparer.Ordinal
                            ),
                    DriveType = DriveType.Fixed,
                    DriveFormat = "NTFS",
                    IsReady = true
                };
            }
        }
    }
}
