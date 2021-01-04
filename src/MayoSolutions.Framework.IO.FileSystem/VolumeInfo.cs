using System.IO;

namespace MayoSolutions.Framework.IO
{
    public class VolumeInfo
    {
        public string RootPathName { get; internal set; }
        public string MappedPathName { get; internal set; }
        public string VolumeLabel { get; internal set; }
        public bool IsCaseSensitive { get; internal set; }
        public bool IsCompressed { get; internal set; }
        public bool SupportsCompression { get; internal set; }
        public bool SupportsEncryption { get; internal set; }
        public bool SupportsHardLinks { get; internal set; }
        public bool IsReady { get; internal set; }
        public long AvailableFreeSpace { get; internal set; }
        public long TotalFreeSpace { get; internal set; }
        public long TotalSize { get; internal set; }
        public string DriveFormat { get; internal set; }
        public DriveType DriveType { get; internal set; }
    }
}