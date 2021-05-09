using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MayoSolutions.Framework.IO.Tests.Bench
{
    public class FileSystemTests
    {
        [Test]
        public void EnsureMappedDrivesAreIncluded()
        {
            var fs = new FileSystem();
            var drives = fs.Drive.GetDrives();
        }
    }
}
