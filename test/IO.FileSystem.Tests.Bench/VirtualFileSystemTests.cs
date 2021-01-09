using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MayoSolutions.Framework.IO.Tests.Bench
{
    public class VirtualFileSystemTests
    {
        [Test]
        public void FixFolderNotReady()
        {
            var vfs = VirtualFileSystem
                .FromPhysicalPath(@"F:\")
                .AndPhysicalPath(@"F:\_TorrentCache")
                ;
        }

        [Test]
        public void FixSequenceContainsMultipleBug()
        {
            var vfs = VirtualFileSystem
                .WithPath(@"F:\")
                .AndPath(@"F:\_TorrentCache")
                .AndPath(@"F:\_TorrentCache\_Completed (local)")
                .AndPath(@"F:\_TorrentCache\_Completed (local)\TV")
                .AndPath(@"F:\_TorrentCache\_Completed (local)\TV\Archer")
                .AndPath(@"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10")
                .WithFiles(new[]
                {
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\01 - 1999- Bort The Garj.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\02 - 1999- Happy Borthday.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\03 - 1999- The Leftovers.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\04 - 1999- Dining With The Zarglorp.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\05 - 1999- Mr. Deadly Goes To Town.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\06 - 1999- Road Trip.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\07 - 1999- Space Pirates.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\08 - 1999- Cubert.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\09 - 1999- Robert De Niro.mp4"
                });
        }


        [Test]
        public void FixSequenceContainsMultipleBug2()
        {
            var vfs = new VirtualFileSystem()
                .WithFiles(new[]
                {
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\01 - 1999- Bort The Garj.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\02 - 1999- Happy Borthday.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\03 - 1999- The Leftovers.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\04 - 1999- Dining With The Zarglorp.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\05 - 1999- Mr. Deadly Goes To Town.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\06 - 1999- Road Trip.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\07 - 1999- Space Pirates.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\08 - 1999- Cubert.mp4",
                    @"F:\_TorrentCache\_Completed (local)\TV\Archer\Season 10\09 - 1999- Robert De Niro.mp4"
                });
        }



	}
}
