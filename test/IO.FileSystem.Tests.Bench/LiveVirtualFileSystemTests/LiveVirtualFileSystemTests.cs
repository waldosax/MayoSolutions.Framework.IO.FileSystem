using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MayoSolutions.Framework.IO;
using NUnit.Framework;

namespace MayoSolutions.Framework.IO.Tests.Bench.LiveVirtualFileSystemTests
{

    public class LiveVirtualFileSystemTests
    {

        private string GetPathToLayoutFile(string fileName)
        {
            return Path.GetFullPath(Path.Combine(
                @"LiveVirtualFileSystemTests\Layouts",
                fileName)
            );
        }

        [Test]
        public void TestLoadFromFile()
        {
            Guid guid = Guid.NewGuid();
            DateTime now = DateTime.Now;
            string srcFilePath = GetPathToLayoutFile("windows.json");
            string filePath = Path.Combine(Path.GetDirectoryName(srcFilePath), $"{now:yyyy.MM.dd.HH.mm.ss}.windows.{guid:D}.json");
            File.Copy(srcFilePath, filePath, true);
            File.SetCreationTime(filePath, now);
            var options = new LiveVirtualFileSystem.LiveVirtualFileSystemOptions
            {
                FileSystemLayoutFilePath = filePath
            };
            IFileSystem fileSystem = new LiveVirtualFileSystem(options);
        }

        [Test]
        public void TestFileSystemSyncsOnOneFile()
        {
            Guid guid = Guid.NewGuid();
            DateTime now = DateTime.Now;
            string srcFilePath = GetPathToLayoutFile("windows.json");
            string filePath = Path.Combine(Path.GetDirectoryName(srcFilePath), $"{now:yyyy.MM.dd.HH.mm.ss}.windows.{guid:D}.json");
            File.Copy(srcFilePath, filePath, true);
            File.SetCreationTime(filePath, now);
            var options = new LiveVirtualFileSystem.LiveVirtualFileSystemOptions
            {
                FileSystemLayoutFilePath = filePath
            };

            var fileSystem1 = new LiveVirtualFileSystem(options);
            Console.WriteLine($"FileSystem 1 is {fileSystem1.GetHashCode()}");
            var allFiles1 = GetAllFiles(fileSystem1);
            Assert.That(allFiles1.Count, Is.EqualTo(4));

            var fileSystem2 = new LiveVirtualFileSystem(options);
            Console.WriteLine($"FileSystem 2 is {fileSystem2.GetHashCode()}");
            var allFiles2 = GetAllFiles(fileSystem2);
            Assert.That(allFiles2.Count, Is.EqualTo(4));


            string hostsFilePath = @"C:\Windows\system32\drivers\etc\hosts";
            string hostsFileDirectory = Path.GetDirectoryName(hostsFilePath);
            Console.WriteLine($"{fileSystem1.GetHashCode()}: CreateDirectory(path:{hostsFileDirectory})");
            fileSystem1.Directory.CreateDirectory(hostsFileDirectory);
            Console.WriteLine($"{fileSystem1.GetHashCode()}: WriteAllText(path:{hostsFilePath})");
            fileSystem1.File.WriteAllText(hostsFilePath, "localhost\t127.0.0.1");

            Thread.Sleep(200);
            while (fileSystem2.Persistence.IsLoading) Thread.Sleep(100);
            Thread.Sleep(200);


            allFiles1 = GetAllFiles(fileSystem1);
            allFiles2 = GetAllFiles(fileSystem2);
            Assert.That(allFiles1.Count, Is.EqualTo(5));
            Assert.That(allFiles2.Count, Is.EqualTo(5));
        }

        private List<string> GetAllFiles(IFileSystem fileSystem)
        {
            List<string> filePaths = new List<string>();

            var drives = fileSystem.Drive.GetDrives();
            foreach (var drive in drives)
            {
                GetAllFiles(fileSystem, drive, ref filePaths);
            }

            return filePaths;
        }

        private List<string> GetAllFiles(IFileSystem fileSystem, string directory, ref List<string> filePaths)
        {
            if (filePaths == null) filePaths = new List<string>();
            foreach (var childDirectory in fileSystem.Directory.GetDirectories(directory))
            {
                GetAllFiles(fileSystem, childDirectory, ref filePaths);
            }
            filePaths.AddRange(fileSystem.Directory.GetFiles(directory));

            return filePaths;
        }
    }
}
