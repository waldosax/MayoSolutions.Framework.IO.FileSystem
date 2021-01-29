using System.Runtime.InteropServices;

namespace MayoSolutions.Framework.IO
{
    public static class OperatingSystem
    {
        public static OSPlatform Platform { get; private set; }
        static OperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Platform = OSPlatform.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Platform = OSPlatform.OSX;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Platform = OSPlatform.Linux;
        }
    }
}
