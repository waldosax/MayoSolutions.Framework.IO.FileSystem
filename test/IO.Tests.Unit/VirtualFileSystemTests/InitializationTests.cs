using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace MayoSolutions.Framework.IO.Tests.Unit.VirtualFileSystemTests
{
    namespace InitializationTests
    {
        public abstract class WhenInitializing
        {
            protected virtual string[] DumpFiles(VirtualFileSystem vfs)
            {
                List<string> dump = new List<string>();
                foreach (var drive in vfs.Drive.GetDrives())
                {
                    DumpFiles(dump, vfs, drive);
                }

                return dump.ToArray();
            }

            private void DumpFiles(List<string> dump, VirtualFileSystem vfs, string folder)
            {
                foreach (var file in vfs.Directory.GetFiles(folder))
                {
                    dump.Add(file);
                }
                foreach (var child in vfs.Directory.GetDirectories(folder))
                {
                    DumpFiles(dump, vfs, child);
                }
            }

            protected void AssertThatFileSystemContainsAllFiles(VirtualFileSystem vfs, string[] expected)
            {
                var actual = DumpFiles(vfs);
                foreach (var path in expected)
                {
                    Assert.That(actual, Contains.Item(path));
                }
            }
        }

        public class WithWindows : WhenInitializing
        {
            [Test]
            public void CanBeInitializedWithWindowsPaths()
            {
                // Arrange
                var sut = new VirtualFileSystem(OSPlatform.Windows);
                var files = new[]
                {
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 11\01 - Archersaurus.mkv",
                };

                // Act
                sut.WithFiles(files);

                // Assert
                AssertThatFileSystemContainsAllFiles(sut, files);
            }
        }

        public class WhenLinux : WhenInitializing
        {
            [Test]
            public void CanBeInitializedWithWindowsPaths()
            {
                // Arrange
                var sut = new VirtualFileSystem(OSPlatform.Linux);
                var files = new[]{
                    @"/mnt/Staging/TV/Archer/Season 11/01 - Archersaurus.mkv",
                };

                // Act
                sut.WithFiles(files);

                // Assert
                AssertThatFileSystemContainsAllFiles(sut, files);
            }

        }
    }
}
