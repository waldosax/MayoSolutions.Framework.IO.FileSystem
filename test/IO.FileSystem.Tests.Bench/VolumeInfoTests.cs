using System;
using NUnit.Framework;

namespace MayoSolutions.Framework.IO.Tests.Bench
{
    public class VolumeInfoTests
    {
        
        [Test]
        public void TestVolumeInfoActual()
        {
            IFileSystem fs = new FileSystem();
            var vi = fs.Drive.GetVolumeInfo("C:");
            DumpVolumeInfo(vi);
            vi = fs.Drive.GetVolumeInfo("D:");
            DumpVolumeInfo(vi);
            vi = fs.Drive.GetVolumeInfo("F:");
            DumpVolumeInfo(vi);
        }
        
        [Test]
        public void TestVolumeInfoVirtual()
        {
            IFileSystem fs = VirtualFileSystem.FromPhysicalPath("C:");
            var vi = fs.Drive.GetVolumeInfo("C:");
            DumpVolumeInfo(vi);
        }

        private void DumpVolumeInfo(VolumeInfo vi)
        {
            Console.WriteLine($"vi.RootPathName: {vi.RootPathName}");
            Console.WriteLine($"vi.VolumeLabel: {vi.VolumeLabel}");
            Console.WriteLine($"vi.MappedPathName: {vi.MappedPathName}");
            Console.WriteLine($"vi.DriveType: {vi.DriveType}");
            Console.WriteLine($"vi.DriveFormat: {vi.DriveFormat}");
            Console.WriteLine($"vi.AvailableFreeSpace: {vi.AvailableFreeSpace}");
            Console.WriteLine($"vi.IsReady: {vi.IsReady}");
            Console.WriteLine($"vi.TotalFreeSpace: {vi.TotalFreeSpace}");
            Console.WriteLine($"vi.TotalSize: {vi.TotalSize}");
            Console.WriteLine($"vi.IsCaseSensitive: {vi.IsCaseSensitive}");
            Console.WriteLine($"vi.IsCompressed: {vi.IsCompressed}");
            Console.WriteLine($"vi.SupportsCompression: {vi.SupportsCompression}");
            Console.WriteLine($"vi.SupportsEncryption: {vi.SupportsEncryption}");
            Console.WriteLine($"vi.SupportsHardLinks: {vi.SupportsHardLinks}");
        }
    }
}