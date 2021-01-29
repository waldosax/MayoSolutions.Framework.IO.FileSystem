using NUnit.Framework;

namespace MayoSolutions.Framework.IO.Tests.Unit
{
    namespace FileSystemUtilityTests
    {
        public abstract class ParsePathTests
        {
            public abstract class WhenLinux : ParsePathTests
            {
                public class AndHasRemoteNodeAsFirstNode : WhenWindows
                {
                    [Test]
                    public void ShouldReturnAllNodes()
                    {
                        TestPath('/', @"//tsclient", @"//tsclient");
                        TestPath('/', @"//tsclient/", @"//tsclient");
                        TestPath('/', @"//tsclient/C", @"//tsclient", "C");
                        TestPath('/', @"//tsclient/C/", @"//tsclient", "C");
                        TestPath('/', @"//tsclient/C/logs", @"//tsclient", "C", "logs");
                        TestPath('/', @"//tsclient/C/logs/", @"//tsclient", "C", "logs");
                        TestPath('/', @"//tsclient/C/logs/2021-01-28", @"//tsclient", "C", "logs", "2021-01-28");
                        TestPath('/', @"//tsclient/C/logs/2021-01-28/", @"//tsclient", "C", "logs", "2021-01-28");
                    }
                }

                public class AndHasRootAsFirstNode : WhenWindows
                {
                    [Test]
                    public void ShouldReturnAllNodes()
                    {
                        TestPath('/', @"/mnt", @"/", "mnt");
                        TestPath('/', @"/mnt/", @"/", "mnt");
                        TestPath('/', @"/mnt/Staging", @"/", "mnt", "Staging");
                        TestPath('/', @"/mnt/Staging/", @"/", "mnt", "Staging");
                        TestPath('/', @"/mnt/Staging/TV", @"/", "mnt", "Staging", "TV");
                        TestPath('/', @"/mnt/Staging/TV/", @"/", "mnt", "Staging", "TV");
                        TestPath('/', @"/mnt/Staging/TV/Archer", @"/", "mnt", "Staging", "TV", "Archer");
                        TestPath('/', @"/mnt/Staging/TV/Archer/", @"/", "mnt", "Staging", "TV", "Archer");
                        TestPath('/', @"/mnt/Staging/TV/Archer/Season 1", @"/", "mnt", "Staging", "TV", "Archer", "Season 1");
                        TestPath('/', @"/mnt/Staging/TV/Archer/Season 1/", @"/", "mnt", "Staging", "TV", "Archer", "Season 1");
                    }
                }
            }

            public abstract class WhenWindows : ParsePathTests
            {
                public class AndHasVolumeAsFirstNode : WhenWindows
                {
                    [Test]
                    public void ShouldReturnAllNodes()
                    {
                        TestPath('\\', @"C:", "C:");
                        TestPath('\\', @"C:\", "C:");
                        TestPath('\\', @"C:\Windows", "C:", "Windows");
                        TestPath('\\', @"C:\Windows\", "C:", "Windows");
                        TestPath('\\', @"C:\Windows\system", "C:", "Windows", "system");
                        TestPath('\\', @"C:\Windows\system\", "C:", "Windows", "system");
                        TestPath('\\', @"C:\Windows\system32\drivers", "C:", "Windows", "system32", "drivers");
                        TestPath('\\', @"C:\Windows\system32\drivers\", "C:", "Windows", "system32", "drivers");
                    }

                }
                public class AndHasMappedDriveAsFirstNode : WhenWindows
                {
                    [Test]
                    public void ShouldReturnAllNodes()
                    {
                        TestPath('\\', @"\\tsclient", @"\\tsclient");
                        TestPath('\\', @"\\tsclient\", @"\\tsclient");
                        TestPath('\\', @"\\tsclient\C", @"\\tsclient", "C");
                        TestPath('\\', @"\\tsclient\C\", @"\\tsclient", "C");
                        TestPath('\\', @"\\tsclient\C\logs", @"\\tsclient", "C", "logs");
                        TestPath('\\', @"\\tsclient\C\logs\", @"\\tsclient", "C", "logs");
                        TestPath('\\', @"\\tsclient\C\logs\2021-01-28", @"\\tsclient", "C", "logs", "2021-01-28");
                        TestPath('\\', @"\\tsclient\C\logs\2021-01-28\", @"\\tsclient", "C", "logs", "2021-01-28");
                    }

                }
            }

            protected void TestPath(char directorySeparatorChar, string path, params string[] expected)
            {
                string[] actual = FileSystemUtility.ParsePath(path, directorySeparatorChar);
                AssertArraysAreEqual(actual, expected);
            }

            protected void AssertArraysAreEqual(string[] actual, params string[] expected)
            {
                Assert.That(expected, Is.Not.Null, () => "Test setup failed.");
                Assert.That(actual, Is.Not.Null);
                Assert.That(actual, Is.EquivalentTo(expected));
            }
        }

        public abstract class SanitizePathTests
        {
            public class WhenWindows : SanitizePathTests
            {
                [Test]
                [TestCase('\\', @"C:\Windows\system32", @"C:\Windows\system32")]
                [TestCase('\\', @"C:\Windows/system32", @"C:\Windows\system32")]
                [TestCase('\\', @"\\tsclient\C$\Windows", @"\\tsclient\C$\Windows")]
                [TestCase('\\', @"\\tsclient\C$/Windows", @"\\tsclient\C$\Windows")]
                public void ShouldSanitizeLinuxPaths(
                    char directorySeparator,
                    string sut,
                    string expected
                    )
                {
                    string actual = FileSystemUtility.SanitizePath(sut, directorySeparator);
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }

            public class WhenLinux : SanitizePathTests
            {
                [Test]
                [TestCase('/', @"/mnt/Staging/TV/Archer", @"/mnt/Staging/TV/Archer")]
                [TestCase('/', @"/mnt/Staging/TV\Archer", @"/mnt/Staging/TV/Archer")]
                [TestCase('/', @"C:\Windows/system32", @"/C/Windows/system32")]
                [TestCase('/', @"//tsclient/C$/Windows", @"//tsclient/C$/Windows")]
                [TestCase('/', @"//tsclient\C$/Windows", @"//tsclient/C$/Windows")]
                public void ShouldSanitizeLinuxPaths(
                    char directorySeparator,
                    string sut,
                    string expected
                    )
                {
                    string actual = FileSystemUtility.SanitizePath(sut, directorySeparator);
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }
        }
    }
}
